using BankApi.Application.DTOs.Requests;
using BankApi.Application.DTOs.Responses;

namespace BankApi.Application.Interfaces;

public interface IBankAccountService
{
    Task<IReadOnlyList<BankAccountResponse>> GetAllAsync(CancellationToken ct = default);
    Task<BankAccountResponse> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<BankAccountResponse> CreateAsync(CreateAccountRequest request, CancellationToken ct = default);
    Task<BankAccountResponse> UpdateAsync(Guid id, UpdateAccountRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<BankAccountResponse> DepositAsync(Guid id, decimal amount, CancellationToken ct = default);
    Task<BankAccountResponse> WithdrawAsync(Guid id, decimal amount, CancellationToken ct = default);
    Task<(BankAccountResponse Sender, BankAccountResponse Receiver)> TransferAsync(Guid senderId, Guid receiverId, decimal amount, CancellationToken ct = default);
}