namespace FabricX.Api.Common;

public class WebResponse<T>
{
    public T? Data { get; private set; }
    public string? Message { get; private set; }
    public bool IsSuccess { get; private set; }
    public int StatusCode { get; private set; }
    public IReadOnlyList<string> Errors { get; private set; }

    private WebResponse(T? data, bool isSuccess, int statusCode, string? message = null, IReadOnlyList<string>? errors = null)
    {
        Data = data;
        IsSuccess = isSuccess;
        StatusCode = statusCode;
        Message = message;
        Errors = errors ?? [];
    }

    public static WebResponse<T> Success(T data, int statusCode = 200, string? message = null)
        => new(data, true, statusCode, message);

    public static WebResponse<T> Failure(string message, int statusCode, IReadOnlyList<string>? errors = null)
        => new(default, false, statusCode, message, errors ?? [message]);
}
