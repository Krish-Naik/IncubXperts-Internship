namespace BankApi.Application.DTOs.Responses;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = default!;
    public T? Data { get; set; }

    public static ApiResponse<T> Ok(T? data, string message = "Request successful")
        => new() { Success = true, Message = message, Data = data };
}