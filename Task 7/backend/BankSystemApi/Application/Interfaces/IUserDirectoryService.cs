using BankApi.Application.DTOs.Responses;

namespace BankApi.Application.Interfaces;

public interface IUserDirectoryService
{
    Task<IReadOnlyList<CustomerLookupResponse>> SearchCustomersAsync(
        string? search,
        CancellationToken ct = default
    );
}
