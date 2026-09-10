using System.Security.Claims;
using asp_gather_match.Contracts.Activities;
using asp_gather_match.Contracts.Common;
using asp_gather_match.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace asp_gather_match.Controllers;

[ApiController]
[Route("api/activities")]
[Authorize]
public class ActivitiesController(IActivityService activityService) : ControllerBase
{
    [HttpGet("{id:long}")]
    [ProducesResponseType<ApiResponse<ActivityResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ActivityResponse>>> Get(long id)
    {
        if (!long.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var hostUserId))
        {
            return Unauthorized(ApiResponse<object>.Fail(
                new ApiError("unauthorized", "需要登入才能執行此操作。"), HttpContext.TraceIdentifier));
        }

        var activity = await activityService.GetAsync(hostUserId, id);
        if (activity is null)
        {
            return NotFound(ApiResponse<object>.Fail(
                new ApiError("not_found", "找不到指定的資源。"), HttpContext.TraceIdentifier));
        }

        return Ok(ApiResponse<ActivityResponse>.Ok(activity, HttpContext.TraceIdentifier));
    }

    [HttpPost]
    [ProducesResponseType<ApiResponse<CreateActivityResponse>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<CreateActivityResponse>>> Create(
        CreateActivityRequest request)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!long.TryParse(userIdValue, out var hostUserId))
        {
            return StatusCode(
                StatusCodes.Status401Unauthorized,
                ApiResponse<object>.Fail(
                    new ApiError("unauthorized", "需要登入才能執行此操作。"),
                    HttpContext.TraceIdentifier));
        }

        var result = await activityService.CreateAsync(hostUserId, request);
        if (!result.Succeeded)
        {
            return BadRequest(ApiResponse<object>.Fail(
                new ApiError(
                    "validation_failed",
                    "輸入資料驗證失敗。",
                    result.Errors),
                HttpContext.TraceIdentifier));
        }

        return Created(
            $"/api/activities/{result.Activity!.Id}",
            ApiResponse<CreateActivityResponse>.Ok(
                result.Activity,
                HttpContext.TraceIdentifier));
    }
}
