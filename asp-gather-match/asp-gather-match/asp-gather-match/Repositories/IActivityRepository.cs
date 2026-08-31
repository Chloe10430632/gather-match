using ActivityEntity = asp_gather_match.Models.Activity;

namespace asp_gather_match.Repositories;

public interface IActivityRepository
{
    Task<bool> ActiveActivityTypeExistsAsync(short activityTypeId);
    Task<bool> ActiveCityExistsAsync(short cityId);
    Task<bool> ActiveDistrictBelongsToCityAsync(int districtId, short cityId);
    Task AddAsync(ActivityEntity activity);
}
