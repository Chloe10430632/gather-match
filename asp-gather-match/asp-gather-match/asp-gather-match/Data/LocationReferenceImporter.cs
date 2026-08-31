using System.Text.Json;
using System.Text.RegularExpressions;
using asp_gather_match.Models;
using Microsoft.EntityFrameworkCore;

namespace asp_gather_match.Data;

public partial class LocationReferenceImporter(
    ApplicationDbContext dbContext,
    ILogger<LocationReferenceImporter> logger)
{
    public async Task<LocationReferenceImportResult> ImportAsync(
        string snapshotPath,
        CancellationToken cancellationToken = default)
    {
        var snapshot = await LoadAndValidateAsync(snapshotPath, cancellationToken);
        var sourceUpdatedAt = snapshot.GeneratedAt;

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var existingCities = await dbContext.Cities
            .Include(city => city.Districts)
            .ToListAsync(cancellationToken);
        var citiesByCode = existingCities.ToDictionary(city => city.GovernmentCode);
        var districtsByCode = existingCities
            .SelectMany(city => city.Districts)
            .ToDictionary(district => district.GovernmentCode);

        var activeCityCodes = snapshot.Cities
            .Select(city => city.GovernmentCode)
            .ToHashSet();
        var activeDistrictCodes = snapshot.Cities
            .SelectMany(city => city.Districts)
            .Select(district => district.GovernmentCode)
            .ToHashSet();

        var addedCities = 0;
        var updatedCities = 0;
        var deactivatedCities = 0;
        var addedDistricts = 0;
        var updatedDistricts = 0;
        var deactivatedDistricts = 0;

        foreach (var citySnapshot in snapshot.Cities)
        {
            var isNewCity = false;
            if (!citiesByCode.TryGetValue(citySnapshot.GovernmentCode, out var city))
            {
                city = new City
                {
                    GovernmentCode = citySnapshot.GovernmentCode
                };
                dbContext.Cities.Add(city);
                citiesByCode.Add(city.GovernmentCode, city);
                addedCities++;
                isNewCity = true;
            }

            if (!isNewCity && (city.Name != citySnapshot.Name ||
                city.SortOrder != citySnapshot.SortOrder ||
                !city.IsActive))
            {
                updatedCities++;
            }

            city.Name = citySnapshot.Name;
            city.SortOrder = citySnapshot.SortOrder;
            city.IsActive = true;
            city.SourceUpdatedAt = sourceUpdatedAt;

            foreach (var districtSnapshot in citySnapshot.Districts)
            {
                var isNewDistrict = false;
                if (!districtsByCode.TryGetValue(districtSnapshot.GovernmentCode, out var district))
                {
                    district = new District
                    {
                        GovernmentCode = districtSnapshot.GovernmentCode,
                        City = city
                    };
                    dbContext.Districts.Add(district);
                    districtsByCode.Add(district.GovernmentCode, district);
                    addedDistricts++;
                    isNewDistrict = true;
                }

                if (!isNewDistrict && (district.Name != districtSnapshot.Name ||
                    district.SortOrder != districtSnapshot.SortOrder ||
                    district.City != city ||
                    !district.IsActive))
                {
                    updatedDistricts++;
                }

                district.Name = districtSnapshot.Name;
                district.SortOrder = districtSnapshot.SortOrder;
                district.City = city;
                district.IsActive = true;
                district.SourceUpdatedAt = sourceUpdatedAt;
            }
        }

        foreach (var city in existingCities.Where(city =>
                     city.IsActive && !activeCityCodes.Contains(city.GovernmentCode)))
        {
            city.IsActive = false;
            city.SourceUpdatedAt = sourceUpdatedAt;
            deactivatedCities++;
        }

        foreach (var district in existingCities
                     .SelectMany(city => city.Districts)
                     .Where(district =>
                         district.IsActive &&
                         !activeDistrictCodes.Contains(district.GovernmentCode)))
        {
            district.IsActive = false;
            district.SourceUpdatedAt = sourceUpdatedAt;
            deactivatedDistricts++;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        var result = new LocationReferenceImportResult(
            addedCities,
            updatedCities,
            deactivatedCities,
            addedDistricts,
            updatedDistricts,
            deactivatedDistricts,
            snapshot.Cities.Count,
            snapshot.Cities.Sum(city => city.Districts.Count));

        logger.LogInformation(
            "Location reference import completed. {@Result}",
            result);

        return result;
    }

    public static async Task<LocationReferenceSnapshot> LoadAndValidateAsync(
        string snapshotPath,
        CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(snapshotPath);
        var snapshot = await JsonSerializer.DeserializeAsync<LocationReferenceSnapshot>(
            stream,
            new JsonSerializerOptions(JsonSerializerDefaults.Web),
            cancellationToken)
            ?? throw new InvalidDataException("位置參照資料快照無法解析。");

        if (!snapshot.IsCompleteSnapshot)
        {
            throw new InvalidDataException("只允許匯入完整位置資料快照。");
        }

        if (snapshot.GeneratedAt == default || snapshot.Cities.Count != 22)
        {
            throw new InvalidDataException("位置資料必須包含產生時間與 22 個縣市。 ");
        }

        if (snapshot.Cities.Any(city =>
                !CityCodeRegex().IsMatch(city.GovernmentCode) ||
                string.IsNullOrWhiteSpace(city.Name) ||
                city.Districts.Count == 0 ||
                city.Districts.Any(district =>
                    !district.GovernmentCode.StartsWith(city.GovernmentCode))))
        {
            throw new InvalidDataException("縣市資料格式不正確。");
        }

        var cityCodes = snapshot.Cities.Select(city => city.GovernmentCode).ToList();
        if (cityCodes.Count != cityCodes.Distinct().Count())
        {
            throw new InvalidDataException("縣市代碼不可重複。");
        }

        var districts = snapshot.Cities.SelectMany(city => city.Districts).ToList();
        if (districts.Count < 350 || districts.Any(district =>
                !DistrictCodeRegex().IsMatch(district.GovernmentCode) ||
                string.IsNullOrWhiteSpace(district.Name)))
        {
            throw new InvalidDataException("行政區資料格式不正確或筆數不足。");
        }

        var districtCodes = districts.Select(district => district.GovernmentCode).ToList();
        if (districtCodes.Count != districtCodes.Distinct().Count())
        {
            throw new InvalidDataException("行政區代碼不可重複。");
        }

        return snapshot;
    }

    [GeneratedRegex("^\\d{5}$")]
    private static partial Regex CityCodeRegex();

    [GeneratedRegex("^\\d{8}$")]
    private static partial Regex DistrictCodeRegex();
}
