namespace techviet_be.Common;

public class ApiResponse<T>
{
    public int Code { get; set; }
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
}

public static class ApiResponse
{
    public static ApiResponse<T> Success<T>(T data, string? message = null, int code = 200)
        => new()
        {
            Code = code,
            IsSuccess = true,
            Data = data,
            Message = message
        };

    public static ApiResponse<object> Error(string message, int code = 500)
        => new()
        {
            Code = code,
            IsSuccess = false,
            Data = null,
            Message = message
        };
}
