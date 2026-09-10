using asp_gather_match.Data;
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
