using asp_gather_match.Contracts.Auth;
using asp_gather_match.Contracts.Common;
using asp_gather_match.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace asp_gather_match.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager) : ControllerBase
{
    [Microsoft.AspNetCore.Authorization.Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<AuthUserResponse>>> Me()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null || !user.IsActive) return Unauthorized();
        return Ok(ApiResponse<AuthUserResponse>.Ok(ToResponse(user), HttpContext.TraceIdentifier));
    }

    [HttpPost("register")]
    [ProducesResponseType<ApiResponse<AuthUserResponse>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<AuthUserResponse>>> Register(
        RegisterRequest request)
    {
        var email = request.Email.Trim();
        var displayName = request.DisplayName.Trim();

        if (displayName.Length == 0)
        {
            return ValidationError(
                new Dictionary<string, string[]>
                {
                    [nameof(request.DisplayName)] = ["顯示名稱不可只包含空白。"]
                });
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            DisplayName = displayName
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors
                .GroupBy(error => error.Code)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(error => error.Description).ToArray());

            return ValidationError(errors);
        }

        var response = ToResponse(user);
        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponse<AuthUserResponse>.Ok(
                response,
                HttpContext.TraceIdentifier));
    }

    [HttpPost("login")]
    [ProducesResponseType<ApiResponse<AuthUserResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<AuthUserResponse>>> Login(
        LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null || !user.IsActive)
        {
            return LoginFailed();
        }

        var result = await signInManager.PasswordSignInAsync(
            user,
            request.Password,
            isPersistent: false,
            lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            return LoginFailed();
        }

        return Ok(ApiResponse<AuthUserResponse>.Ok(
            ToResponse(user),
            HttpContext.TraceIdentifier));
    }

    [HttpPost("logout")]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> Logout()
    {
        // 可重複呼叫，讓已過期或已登出的瀏覽器也能清除 Cookie。
        await signInManager.SignOutAsync();
        return Ok(ApiResponse<object>.Ok(new { }, HttpContext.TraceIdentifier));
    }

    private BadRequestObjectResult ValidationError(
        Dictionary<string, string[]> errors) =>
        BadRequest(ApiResponse<object>.Fail(
            new ApiError(
                "validation_failed",
                "輸入資料驗證失敗。",
                errors),
            HttpContext.TraceIdentifier));

    private UnauthorizedObjectResult LoginFailed() =>
        Unauthorized(ApiResponse<object>.Fail(
            new ApiError(
                "invalid_credentials",
                "Email 或密碼不正確。"),
            HttpContext.TraceIdentifier));

    private static AuthUserResponse ToResponse(ApplicationUser user) =>
        new(
            user.Id,
            user.Email ?? string.Empty,
            user.DisplayName);
}
