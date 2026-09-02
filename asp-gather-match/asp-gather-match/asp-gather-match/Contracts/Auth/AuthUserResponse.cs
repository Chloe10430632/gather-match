namespace asp_gather_match.Contracts.Auth;

public record AuthUserResponse(
    long Id,
    string Email,
    string DisplayName);
