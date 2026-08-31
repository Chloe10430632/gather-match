namespace asp_gather_match.Contracts.Common;

public class ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public ApiError? Error { get; init; }
    public string TraceId { get; init; } = string.Empty;

    public static ApiResponse<T> Ok(T data, string traceId) => new()
    {
        Success = true,
        Data = data,
        TraceId = traceId
    };

    public static ApiResponse<T> Fail(ApiError error, string traceId) => new()
    {
        Success = false,
        Error = error,
        TraceId = traceId
    };
}
