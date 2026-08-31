using asp_gather_match.Contracts.Activities;

namespace asp_gather_match.Services;

public interface IActivityService
{
    Task<ActivityCreationResult> CreateAsync(
        long hostUserId,
        CreateActivityRequest request);
}
