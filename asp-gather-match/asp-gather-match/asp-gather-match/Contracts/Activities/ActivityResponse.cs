namespace asp_gather_match.Contracts.Activities;

public record ActivityResponse(
    long Id, string Title, short ActivityTypeId,
    decimal? BudgetMin, decimal? BudgetMax, string CurrencyCode,
    short CityId, int? DistrictId, DateTimeOffset DeadlineAt, string Status,
    DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt,
    IReadOnlyList<DateOptionResponse> DateOptions,
    IReadOnlyList<PlaceOptionResponse> PlaceOptions);

public record DateOptionResponse(
    long Id, DateOnly OptionDate, TimeOnly? StartTime, TimeOnly? EndTime, short SortOrder);

public record PlaceOptionResponse(
    long Id, string SourceType, string DisplayLabel, string? CustomAddress, short SortOrder);
