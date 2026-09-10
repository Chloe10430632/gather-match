using System.Text.Json;
using asp_gather_match.Contracts.Activities;
using asp_gather_match.Models;
using asp_gather_match.Repositories;
using asp_gather_match.Services;
using Moq;

namespace GatherMatch.Tests;

public class ActivityUpdateTests
{
    private readonly Mock<IActivityRepository> repository = new();
    private readonly ActivityService service;
    private readonly Activity activity = SampleActivity();

    public ActivityUpdateTests()
    {
        repository.Setup(x => x.GetOwnedAsync(42, 7)).ReturnsAsync(activity);
        repository.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);
        service = new(repository.Object);
    }

    internal static Activity SampleActivity() => new()
    {
        Id = 42, HostUserId = 7, Title = "Original", ActivityTypeId = 1,
        BudgetMin = 100, BudgetMax = 500, CityId = 1, DistrictId = null,
        DeadlineAt = DateTimeOffset.UtcNow.AddDays(2), Status = "open",
        ShareTokenHash = Guid.NewGuid().ToString("N"),
        CreatedAt = DateTimeOffset.UtcNow.AddDays(-1), UpdatedAt = DateTimeOffset.UtcNow.AddDays(-1),
        DateOptions =
        [
            new() { Id = 11, ActivityId = 42, OptionDate = new(2026, 10, 1), StartTime = new(18, 0), EndTime = new(20, 0), SortOrder = 0 },
            new() { Id = 12, ActivityId = 42, OptionDate = new(2026, 10, 2), StartTime = new(18, 0), EndTime = new(20, 0), SortOrder = 1 }
        ],
        PlaceOptions = [new() { Id = 21, SourceType = "custom", DisplayLabel = "Original place" }]
    };

    [Fact]
    public async Task Update_TitleAndDate_PreservesImmutableFieldsAndOptionIdentity()
    {
        var before = await service.GetAsync(7, 42);
        var hash = activity.ShareTokenHash;
        var result = await service.UpdateAsync(7, 42, new()
        {
            Title = " Updated ",
            DateOptions = [new() { Id = 11, OptionDate = new(2026, 10, 3), StartTime = new(19, 0), EndTime = new(21, 0) }]
        });

        Assert.Null(result.ErrorCode);
        var after = Assert.IsType<ActivityResponse>(result.Activity);
        Assert.Equal("Updated", after.Title);
        Assert.Equal(new DateOnly(2026, 10, 3), after.DateOptions[0].OptionDate);
        Assert.Equal(new TimeOnly(19, 0), after.DateOptions[0].StartTime);
        Assert.Equal(new TimeOnly(21, 0), after.DateOptions[0].EndTime);
        Assert.Equal(before!.DateOptions[1], after.DateOptions[1]);
        Assert.Equal(before.DateOptions.Select(x => (x.Id, x.SortOrder)), after.DateOptions.Select(x => (x.Id, x.SortOrder)));
        Assert.Equal(before.PlaceOptions, after.PlaceOptions);
        Assert.Equal((before.ActivityTypeId, before.CityId, before.DistrictId, before.BudgetMin, before.BudgetMax, before.CurrencyCode, before.DeadlineAt, before.Status, before.CreatedAt),
            (after.ActivityTypeId, after.CityId, after.DistrictId, after.BudgetMin, after.BudgetMax, after.CurrencyCode, after.DeadlineAt, after.Status, after.CreatedAt));
        Assert.Equal(hash, activity.ShareTokenHash);
        Assert.Equal(7, activity.HostUserId);
        Assert.True(after.UpdatedAt > before.UpdatedAt);
        repository.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Update_TitleOnly_LeavesDatesUnchanged()
    {
        var before = await service.GetAsync(7, 42);
        var result = await service.UpdateAsync(7, 42, new() { Title = "Renamed" });
        Assert.Equal(before!.DateOptions, result.Activity!.DateOptions);
    }

    [Fact]
    public async Task Update_DateOnly_LeavesTitleAndCanClearTimes()
    {
        var result = await service.UpdateAsync(7, 42, new()
        {
            DateOptions = [new() { Id = 11, OptionDate = new(2026, 10, 3) }]
        });
        Assert.Equal("Original", result.Activity!.Title);
        Assert.Null(result.Activity.DateOptions[0].StartTime);
        Assert.Null(result.Activity.DateOptions[0].EndTime);
    }

    [Fact]
    public async Task Update_DateSwap_ValidatesFinalState()
    {
        var result = await service.UpdateAsync(7, 42, new()
        {
            DateOptions =
            [
                new() { Id = 11, OptionDate = new(2026, 10, 2), StartTime = new(18, 0) },
                new() { Id = 12, OptionDate = new(2026, 10, 1), StartTime = new(18, 0) }
            ]
        });
        Assert.Null(result.ErrorCode);
        repository.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Theory]
    [InlineData("draft", false)]
    [InlineData("closed", false)]
    [InlineData("finalized", false)]
    [InlineData("cancelled", false)]
    [InlineData("open", true)]
    public async Task Update_NotOpenOrExpired_RejectsWithoutMutation(string status, bool expired)
    {
        activity.Status = status;
        if (expired) activity.DeadlineAt = DateTimeOffset.UtcNow;
        var result = await service.UpdateAsync(7, 42, new() { Title = "Changed" });
        Assert.Equal("activity_not_editable", result.ErrorCode);
        Assert.Equal("Original", activity.Title);
        repository.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Theory]
    [InlineData(8, 42)]
    [InlineData(7, 999)]
    public async Task Update_OtherOwnerOrMissing_ReturnsNotFound(long ownerId, long activityId)
    {
        var result = await service.UpdateAsync(ownerId, activityId, new() { Title = "Changed" });
        Assert.Equal("not_found", result.ErrorCode);
        repository.Verify(x => x.GetOwnedAsync(activityId, ownerId), Times.Once);
        repository.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Theory]
    [InlineData("empty")]
    [InlineData("blank-title")]
    [InlineData("long-title")]
    [InlineData("empty-dates")]
    [InlineData("null-date")]
    [InlineData("unknown-id")]
    [InlineData("duplicate-id")]
    [InlineData("duplicate-date")]
    [InlineData("duplicate-null-time")]
    [InlineData("invalid-time")]
    public async Task Update_InvalidInput_DoesNotMutateOrSave(string scenario)
    {
        var request = new UpdateActivityRequest { Title = "Changed" };
        switch (scenario)
        {
            case "empty": request.Title = null; break;
            case "blank-title": request.Title = " "; break;
            case "long-title": request.Title = new string('a', 101); break;
            case "empty-dates": request.DateOptions = []; break;
            case "null-date": request.DateOptions = [null!]; break;
            case "unknown-id": request.DateOptions = [new() { Id = 999, OptionDate = new(2026, 10, 3) }]; break;
            case "duplicate-id": request.DateOptions = [new() { Id = 11 }, new() { Id = 11 }]; break;
            case "duplicate-date": request.DateOptions = [new() { Id = 11, OptionDate = new(2026, 10, 2), StartTime = new(18, 0) }]; break;
            case "duplicate-null-time":
                activity.DateOptions.Last().StartTime = null;
                request.DateOptions = [new() { Id = 11, OptionDate = new(2026, 10, 2) }]; break;
            case "invalid-time": request.DateOptions = [new() { Id = 11, OptionDate = new(2026, 10, 3), StartTime = new(20, 0), EndTime = new(19, 0) }]; break;
        }
        var before = await service.GetAsync(7, 42);
        var result = await service.UpdateAsync(7, 42, request);
        Assert.Equal("validation_failed", result.ErrorCode);
        Assert.NotEmpty(result.Errors);
        var after = await service.GetAsync(7, 42);
        Assert.Equal(JsonSerializer.Serialize(before), JsonSerializer.Serialize(after));
        repository.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Theory]
    [InlineData("deadlineAt")]
    [InlineData("activityTypeId")]
    [InlineData("placeOptions")]
    [InlineData("budgetMin")]
    [InlineData("budgetMax")]
    [InlineData("cityId")]
    [InlineData("districtId")]
    [InlineData("hostUserId")]
    [InlineData("status")]
    public void Request_RejectsImmutableOrUnknownFields(string field)
    {
        var json = JsonSerializer.Serialize(new Dictionary<string, object> { [field] = 1 });
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<UpdateActivityRequest>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
    }
}
