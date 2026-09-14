using asp_gather_match.Contracts.Auth;
using asp_gather_match.Contracts.Common;
using asp_gather_match.Controllers;
using asp_gather_match.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace GatherMatch.Tests;

public class AuthControllerTests
{
    private readonly Mock<UserManager<ApplicationUser>> users = new(
        Mock.Of<IUserStore<ApplicationUser>>(), Options.Create(new IdentityOptions()),
        Mock.Of<IPasswordHasher<ApplicationUser>>(), Array.Empty<IUserValidator<ApplicationUser>>(),
        Array.Empty<IPasswordValidator<ApplicationUser>>(), Mock.Of<ILookupNormalizer>(),
        new IdentityErrorDescriber(), Mock.Of<IServiceProvider>(), Mock.Of<ILogger<UserManager<ApplicationUser>>>());

    private readonly Mock<SignInManager<ApplicationUser>> signIn;
    private readonly AuthController controller;

    public AuthControllerTests()
    {
        signIn = new(users.Object, Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(), Options.Create(new IdentityOptions()),
            Mock.Of<ILogger<SignInManager<ApplicationUser>>>(), Mock.Of<IAuthenticationSchemeProvider>(),
            Mock.Of<IUserConfirmation<ApplicationUser>>());
        controller = new(users.Object, signIn.Object)
        {
            ControllerContext = new() { HttpContext = new DefaultHttpContext() }
        };
    }

    [Fact]
    public async Task Me_ReturnsCurrentUserWithoutCredentials()
    {
        users.Setup(x => x.GetUserAsync(controller.User)).ReturnsAsync(
            new ApplicationUser { Id = 7, Email = "host@example.invalid", DisplayName = "Host", IsActive = true });
        var result = await controller.Me();
        var response = Assert.IsType<ApiResponse<AuthUserResponse>>(Assert.IsType<OkObjectResult>(result.Result).Value);
        Assert.Equal(7, response.Data!.Id);
        Assert.Equal("Host", response.Data.DisplayName);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Me_RejectsMissingOrInactiveUser(bool exists)
    {
        users.Setup(x => x.GetUserAsync(controller.User)).ReturnsAsync(
            exists ? new ApplicationUser { IsActive = false } : null);
        Assert.IsType<UnauthorizedResult>((await controller.Me()).Result);
    }

    [Fact]
    public async Task Login_ValidUser_UsesNonPersistentCookieAndLockout()
    {
        var user = new ApplicationUser { Id = 7, Email = "host@example.invalid", DisplayName = "Test host", IsActive = true };
        users.Setup(x => x.FindByEmailAsync(user.Email)).ReturnsAsync(user);
        signIn.Setup(x => x.PasswordSignInAsync(user, It.IsAny<string>(), false, true))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

        var result = await controller.Login(new LoginRequest { Email = " host@example.invalid ", Password = Guid.NewGuid().ToString() });

        var response = Assert.IsType<ApiResponse<AuthUserResponse>>(Assert.IsType<OkObjectResult>(result.Result).Value);
        Assert.True(response.Success);
        Assert.Equal(7, response.Data!.Id);
        signIn.Verify(x => x.PasswordSignInAsync(user, It.IsAny<string>(), false, true), Times.Once);
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("inactive")]
    [InlineData("wrong-password")]
    [InlineData("locked-out")]
    public async Task Login_Failures_ReturnSamePublicError(string scenario)
    {
        var user = scenario == "missing" ? null : new ApplicationUser { IsActive = scenario != "inactive" };
        users.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
        signIn.Setup(x => x.PasswordSignInAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), false, true))
            .ReturnsAsync(scenario == "locked-out" ? Microsoft.AspNetCore.Identity.SignInResult.LockedOut : Microsoft.AspNetCore.Identity.SignInResult.Failed);

        var result = await controller.Login(new LoginRequest { Email = "host@example.invalid", Password = Guid.NewGuid().ToString() });

        var response = Assert.IsType<ApiResponse<object>>(Assert.IsType<UnauthorizedObjectResult>(result.Result).Value);
        Assert.Equal("invalid_credentials", response.Error!.Code);
        if (scenario is "missing" or "inactive")
            signIn.Verify(x => x.PasswordSignInAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), false, true), Times.Never);
    }

    [Fact]
    public async Task Logout_SignsOutAndCanBeRepeated()
    {
        signIn.Setup(x => x.SignOutAsync()).Returns(Task.CompletedTask);
        for (var i = 0; i < 2; i++)
        {
            var result = await controller.Logout();
            var response = Assert.IsType<ApiResponse<object>>(Assert.IsType<OkObjectResult>(result.Result).Value);
            Assert.True(response.Success);
        }
        signIn.Verify(x => x.SignOutAsync(), Times.Exactly(2));
    }
}
