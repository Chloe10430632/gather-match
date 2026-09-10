using asp_gather_match.Contracts.Activities;

namespace asp_gather_match.Services;

public interface IActivityService
{
    Task<ActivityResponse?> GetAsync(long hostUserId, long activityId);
    Task<ActivityCreationResult> CreateAsync(
        long hostUserId,
        CreateActivityRequest request);
}
