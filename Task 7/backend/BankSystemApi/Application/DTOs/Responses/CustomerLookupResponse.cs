namespace BankApi.Application.DTOs.Responses;

public sealed class CustomerLookupResponse
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
