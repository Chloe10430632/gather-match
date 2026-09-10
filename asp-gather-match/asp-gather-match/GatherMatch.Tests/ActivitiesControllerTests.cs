using System.Security.Claims;
using asp_gather_match.Contracts.Activities;
using asp_gather_match.Contracts.Common;
using asp_gather_match.Controllers;
using asp_gather_match.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GatherMatch.Tests;

public class ActivitiesControllerTests
{
    private readonly Mock<IActivityService> service = new();

    private ActivitiesController Controller(string? userId)
    {
        var context = new DefaultHttpContext();
        if (userId is not null)
            context.User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId)], "test"));
        return new(service.Object) { ControllerContext = new() { HttpContext = context } };
    }

    [Theory]
    [InlineData(null)]
    [InlineData("invalid")]
    public async Task Get_InvalidIdentity_Returns401WithoutCallingService(string? userId)
    {
        var result = await Controller(userId).Get(42);
        Assert.IsType<UnauthorizedObjectResult>(result.Result);
        service.Verify(x => x.GetAsync(It.IsAny<long>(), It.IsAny<long>()), Times.Never);
    }

    [Fact]
    public async Task Get_MissingOrOtherOwner_Returns404Envelope()
    {
        var result = await Controller("7").Get(42);
        var body = Assert.IsType<ApiResponse<object>>(Assert.IsType<NotFoundObjectResult>(result.Result).Value);
        Assert.Equal("not_found", body.Error!.Code);
        service.Verify(x => x.GetAsync(7, 42), Times.Once);
    }

    [Fact]
    public async Task Get_OwnedActivity_Returns200Envelope()
    {
        var activity = new ActivityResponse(42, "Test", 1, null, null, "TWD", 1, null,
            DateTimeOffset.UtcNow.AddDays(1), "open", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, [], []);
        service.Setup(x => x.GetAsync(7, 42)).ReturnsAsync(activity);
        var result = await Controller("7").Get(42);
        var body = Assert.IsType<ApiResponse<ActivityResponse>>(Assert.IsType<OkObjectResult>(result.Result).Value);
        Assert.Equal(activity, body.Data);
    }

    [Fact]
    public async Task Create_UsesClaimAndReturnsResourceLocation()
    {
        var request = ActivityServiceTests.ValidRequest();
        service.Setup(x => x.CreateAsync(7, request)).ReturnsAsync(ActivityCreationResult.Success(new(42, Guid.NewGuid().ToString())));
        var result = await Controller("7").Create(request);
        var created = Assert.IsType<CreatedResult>(result.Result);
        Assert.Equal("/api/activities/42", created.Location);
        service.Verify(x => x.CreateAsync(7, request), Times.Once);
    }
}
