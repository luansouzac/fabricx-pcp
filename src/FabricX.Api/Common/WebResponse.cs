namespace FabricX.Api.Common;

public class WebResponse<T>
{
    public T? Data { get; private set; }
    public string? Message { get; private set; }
    public bool IsSuccess { get; private set; }
    public int StatusCode { get; private set; }

    private WebResponse(T? data, bool isSuccess, int statusCode, string? message = null)
    {
        Data = data;
        IsSuccess = isSuccess;
        StatusCode = statusCode;
        Message = message;
    }

    public static WebResponse<T> Success(T data, int statusCode = 200, string? message = null)
        => new(data, true, statusCode, message);

    public static WebResponse<T> Failure(string message, int statusCode)
        => new(default, false, statusCode, message);
}
