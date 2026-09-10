using System.Security.Cryptography;
using System.Text;
using asp_gather_match.Contracts.Activities;
using asp_gather_match.Models;
using asp_gather_match.Repositories;
using asp_gather_match.Services;
using Moq;

namespace GatherMatch.Tests;

public class ActivityServiceTests
{
    private readonly Mock<IActivityRepository> repository = new();
    private readonly ActivityService service;

    public ActivityServiceTests()
    {
        repository.Setup(x => x.ActiveActivityTypeExistsAsync(1)).ReturnsAsync(true);
        repository.Setup(x => x.ActiveCityExistsAsync(1)).ReturnsAsync(true);
        repository.Setup(x => x.ActiveDistrictBelongsToCityAsync(1, 1)).ReturnsAsync(true);
        service = new(repository.Object);
    }

    internal static CreateActivityRequest ValidRequest() => new()
    {
        Title = " Test activity ", ActivityTypeId = 1, CityId = 1, DistrictId = 1,
        BudgetMin = 100, BudgetMax = 500,
        DeadlineAt = DateTimeOffset.UtcNow.AddDays(2).ToOffset(TimeSpan.FromHours(8)),
        DateOptions = [new() { OptionDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3)), StartTime = new(18, 0), EndTime = new(20, 0) }],
        PlaceOptions = [new() { DisplayLabel = " Test place ", CustomAddress = " Test address " }]
    };

    [Fact]
    public async Task Get_UsesOwnerAndReturnsOrderedOptionsWithoutSecrets()
    {
        repository.Setup(x => x.GetOwnedAsync(42, 7)).ReturnsAsync(new Activity
        {
            Id = 42, HostUserId = 7, Title = "Test activity",
            ShareTokenHash = Guid.NewGuid().ToString(),
            DateOptions = [new() { Id = 2, SortOrder = 1 }, new() { Id = 1, SortOrder = 0 }],
            PlaceOptions = [new() { Id = 4, SortOrder = 1 }, new() { Id = 3, SortOrder = 0 }]
        });

        var result = await service.GetAsync(7, 42);

        Assert.NotNull(result);
        Assert.Equal(42, result.Id);
        Assert.Equal(new long[] { 1, 2 }, result.DateOptions.Select(x => x.Id));
        Assert.Equal(new long[] { 3, 4 }, result.PlaceOptions.Select(x => x.Id));
        Assert.DoesNotContain("token", System.Text.Json.JsonSerializer.Serialize(result), StringComparison.OrdinalIgnoreCase);
        repository.Verify(x => x.GetOwnedAsync(42, 7), Times.Once);
    }

    [Fact]
    public async Task Get_NotOwnedOrMissing_ReturnsNull()
    {
        repository.Setup(x => x.GetOwnedAsync(42, 8)).ReturnsAsync((Activity?)null);
        Assert.Null(await service.GetAsync(8, 42));
    }

    [Fact]
    public async Task Create_PersistsOwnerUtcAndHashOnly()
    {
        Activity? saved = null;
        repository.Setup(x => x.AddAsync(It.IsAny<Activity>())).Callback<Activity>(a => { a.Id = 42; saved = a; }).Returns(Task.CompletedTask);
        var request = ValidRequest();

        var result = await service.CreateAsync(7, request);

        Assert.True(result.Succeeded);
        Assert.Equal(42, result.Activity!.Id);
        Assert.NotNull(saved);
        Assert.Equal(7, saved.HostUserId);
        Assert.Equal("Test activity", saved.Title);
        Assert.Equal("open", saved.Status);
        Assert.Equal(TimeSpan.Zero, saved.DeadlineAt.Offset);
        Assert.Equal(request.DeadlineAt, saved.DeadlineAt);
        Assert.Equal(Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(result.Activity.ShareToken))), saved.ShareTokenHash);
        Assert.NotEqual(result.Activity.ShareToken, saved.ShareTokenHash);
        Assert.Equal("Test place", Assert.Single(saved.PlaceOptions).DisplayLabel);
        Assert.Single(saved.DateOptions);
        repository.Verify(x => x.AddAsync(It.IsAny<Activity>()), Times.Once);
    }

    [Theory]
    [InlineData("title")]
    [InlineData("deadlineAt")]
    [InlineData("budgetMax")]
    [InlineData("empty-dates")]
    [InlineData("duplicate-dates")]
    [InlineData("invalid-times")]
    [InlineData("empty-places")]
    [InlineData("too-many-places")]
    [InlineData("blank-place")]
    [InlineData("activityTypeId")]
    [InlineData("cityId")]
    [InlineData("districtId")]
    public async Task Create_InvalidInput_DoesNotPersist(string scenario)
    {
        var request = ValidRequest();
        var field = scenario;
        switch (scenario)
        {
            case "title": request.Title = " "; break;
            case "deadlineAt": request.DeadlineAt = DateTimeOffset.UtcNow.AddDays(-1); break;
            case "budgetMax": request.BudgetMax = 1; break;
            case "empty-dates": request.DateOptions.Clear(); field = "dateOptions"; break;
            case "duplicate-dates": request.DateOptions.Add(request.DateOptions[0]); field = "dateOptions"; break;
            case "invalid-times": request.DateOptions[0].EndTime = new(17, 0); field = "dateOptions"; break;
            case "empty-places": request.PlaceOptions.Clear(); field = "placeOptions"; break;
            case "too-many-places": request.PlaceOptions = Enumerable.Range(0, 6).Select(_ => new CreatePlaceOptionRequest { DisplayLabel = "Place" }).ToList(); field = "placeOptions"; break;
            case "blank-place": request.PlaceOptions[0].DisplayLabel = " "; field = "placeOptions"; break;
            case "activityTypeId": request.ActivityTypeId = 99; break;
            case "cityId": request.CityId = 99; break;
            case "districtId": request.DistrictId = 99; break;
        }

        var result = await service.CreateAsync(7, request);

        Assert.False(result.Succeeded);
        Assert.Contains(field, result.Errors.Keys);
        repository.Verify(x => x.AddAsync(It.IsAny<Activity>()), Times.Never);
    }
}
