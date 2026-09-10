using asp_gather_match.Data;
using asp_gather_match.Models;
using Microsoft.EntityFrameworkCore;
using ActivityEntity = asp_gather_match.Models.Activity;

namespace asp_gather_match.Repositories;

public class ActivityRepository(ApplicationDbContext dbContext) : IActivityRepository
{
    public Task<ActivityEntity?> GetOwnedAsync(long activityId, long hostUserId) =>
        dbContext.Activities
            .Include(activity => activity.DateOptions)
            .Include(activity => activity.PlaceOptions)
            .SingleOrDefaultAsync(activity => activity.Id == activityId && activity.HostUserId == hostUserId);

    public Task<bool> ActiveActivityTypeExistsAsync(short activityTypeId) =>
        dbContext.ActivityTypes.AnyAsync(type => type.Id == activityTypeId && type.IsActive);

    public async Task SaveChangesAsync()
    {
        var changedDates = dbContext.ChangeTracker.Entries<DateOption>()
            .Where(entry => entry.State == EntityState.Modified &&
                (entry.Property(option => option.OptionDate).IsModified || entry.Property(option => option.StartTime).IsModified))
            .ToList();
        if (changedDates.Count < 2)
        {
            await dbContext.SaveChangesAsync();
            return;
        }

        // PostgreSQL 的唯一索引逐筆檢查。交換兩個日期時，先在同一交易內移至
        // 不衝突的暫存日期，再寫入最終日期；不刪除選項，失敗則整筆交易回復。
        var targets = changedDates.Select(entry => (entry.Entity, entry.Entity.OptionDate)).ToList();
        var reservedDates = dbContext.ChangeTracker.Entries<DateOption>()
            .SelectMany(entry => new[] { entry.Entity.OptionDate, entry.OriginalValues.GetValue<DateOnly>(nameof(DateOption.OptionDate)) })
            .ToHashSet();
        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        var temporaryDate = DateOnly.MinValue;
        foreach (var (option, _) in targets)
        {
            while (reservedDates.Contains(temporaryDate)) temporaryDate = temporaryDate.AddDays(1);
            option.OptionDate = temporaryDate;
            reservedDates.Add(temporaryDate);
        }
        await dbContext.SaveChangesAsync();
        foreach (var (option, targetDate) in targets) option.OptionDate = targetDate;
        await dbContext.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    public Task<bool> ActiveCityExistsAsync(short cityId) =>
        dbContext.Cities.AnyAsync(city => city.Id == cityId && city.IsActive);

    public Task<bool> ActiveDistrictBelongsToCityAsync(int districtId, short cityId) =>
        dbContext.Districts.AnyAsync(district =>
            district.Id == districtId &&
            district.CityId == cityId &&
            district.IsActive);

    public async Task AddAsync(ActivityEntity activity)
    {
        dbContext.Activities.Add(activity);
        await dbContext.SaveChangesAsync();
    }
}
