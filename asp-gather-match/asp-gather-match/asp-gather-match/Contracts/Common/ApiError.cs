namespace asp_gather_match.Contracts.Common;

public record ApiError(
    string Code,
    string Message,
    Dictionary<string, string[]>? Details = null);
