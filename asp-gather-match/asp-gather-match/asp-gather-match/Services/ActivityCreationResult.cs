using asp_gather_match.Contracts.Activities;

namespace asp_gather_match.Services;

public record ActivityCreationResult(
    CreateActivityResponse? Activity,
    Dictionary<string, string[]> Errors)
{
    public bool Succeeded => Activity is not null;

    public static ActivityCreationResult Success(CreateActivityResponse activity) =>
        new(activity, []);

    public static ActivityCreationResult Failure(string field, string message) =>
        new(null, new Dictionary<string, string[]> { [field] = [message] });
}
