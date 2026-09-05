namespace FoodMesh.Shared.SharedDto;

/// <summary>
/// Uniform REST API response contract returned by all endpoints.
/// Ensures frontend clients (Angular, React, Mobile) get a consistent payload structure.
/// </summary>
/// <typeparam name="T">The type of the response data payload.</typeparam>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

    public static ApiResponse<T> Ok(T data, string message = "Success")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static ApiResponse<T> Fail(string message, List<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors ?? [message]
        };
    }
}
