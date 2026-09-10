using asp_gather_match.Contracts.Activities;

namespace asp_gather_match.Services;

public record ActivityUpdateResult(
    ActivityResponse? Activity,
    string? ErrorCode,
    Dictionary<string, string[]> Errors)
{
    public static ActivityUpdateResult Success(ActivityResponse activity) => new(activity, null, []);
    public static ActivityUpdateResult NotFound() => new(null, "not_found", []);
    public static ActivityUpdateResult NotEditable() => new(null, "activity_not_editable", []);
    public static ActivityUpdateResult Invalid(string field, string message) =>
        new(null, "validation_failed", new Dictionary<string, string[]> { [field] = [message] });
}
