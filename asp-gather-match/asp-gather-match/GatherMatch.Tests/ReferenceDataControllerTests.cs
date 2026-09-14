using System.Text.Json;
using asp_gather_match.Contracts.Common;
using asp_gather_match.Controllers;
using asp_gather_match.Data;
using asp_gather_match.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace GatherMatch.Tests;

public class ReferenceDataControllerTests
{
    [Fact]
    public async Task Get_ReturnsActiveReferencesWithActualIdsAndOrderedDistricts()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.Cities.AddRange(
            new City { Id = 42, Name = "Active", GovernmentCode = "00001" },
            new City { Id = 43, Name = "Inactive", GovernmentCode = "00002", IsActive = false });
        db.Districts.AddRange(
            new District { Id = 101, CityId = 42, Name = "Second", GovernmentCode = "00001001", SortOrder = 2 },
            new District { Id = 102, CityId = 42, Name = "First", GovernmentCode = "00001002", SortOrder = 1 },
            new District { Id = 103, CityId = 42, Name = "Inactive", GovernmentCode = "00001003", IsActive = false });
        (await db.ActivityTypes.FirstAsync()).IsActive = false;
        await db.SaveChangesAsync();
        var controller = new ReferenceDataController(db) { ControllerContext = new() { HttpContext = new DefaultHttpContext() } };

        var result = await controller.Get(CancellationToken.None);

        var response = Assert.IsType<ApiResponse<object>>(Assert.IsType<OkObjectResult>(result.Result).Value);
        var json = JsonSerializer.SerializeToElement(response.Data, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        Assert.Equal(5, json.GetProperty("activityTypes").GetArrayLength());
        var cities = json.GetProperty("cities");
        Assert.Equal(1, cities.GetArrayLength());
        Assert.Equal(42, cities[0].GetProperty("id").GetInt32());
        var districts = cities[0].GetProperty("districts");
        Assert.Equal(new[] { 102, 101 }, districts.EnumerateArray().Select(x => x.GetProperty("id").GetInt32()));
    }
}
