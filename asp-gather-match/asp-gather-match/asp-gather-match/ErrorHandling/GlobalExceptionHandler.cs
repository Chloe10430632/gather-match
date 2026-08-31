using asp_gather_match.Contracts.Common;
using Microsoft.AspNetCore.Diagnostics;

namespace asp_gather_match.ErrorHandling;

public class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(
            exception,
            "Unhandled exception. TraceId: {TraceId}",
            httpContext.TraceIdentifier);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var response = ApiResponse<object>.Fail(
            new ApiError(
                "internal_server_error",
                "伺服器發生未預期的錯誤。"),
            httpContext.TraceIdentifier);

        await httpContext.Response.WriteAsJsonAsync(
            response,
            cancellationToken);

        return true;
    }
}
