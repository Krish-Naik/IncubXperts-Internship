using BankApi.Application.DTOs.Requests;
using BankApi.Application.DTOs.Responses;

namespace BankApi.Application.Interfaces;

public interface IBankAccountService
{
    Task<IReadOnlyList<BankAccountResponse>> GetOwnAccountsAsync(
        Guid appUserId,
        CancellationToken ct = default
    );
    Task<IReadOnlyList<BankAccountResponse>> SearchAsync(
        string? search,
        CancellationToken ct = default
    );
    Task<BankAccountResponse> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<BankAccountResponse> CreateAsync(
        CreateAccountRequest request,
        Guid actorUserId,
        string actorRole,
        CancellationToken ct = default
    );
    Task<BankAccountResponse> UpdateAsync(
        Guid id,
        UpdateAccountRequest request,
        Guid actorUserId,
        string actorRole,
        CancellationToken ct = default
    );
    Task<BankAccountResponse> CloseAsync(
        Guid id,
        Guid actorUserId,
        string actorRole,
        CancellationToken ct = default
    );

    Task<BankAccountResponse> DepositOwnAsync(
        Guid appUserId,
        Guid accountId,
        decimal amount,
        CancellationToken ct = default
    );
    Task<BankAccountResponse> WithdrawOwnAsync(
        Guid appUserId,
        Guid accountId,
        decimal amount,
        CancellationToken ct = default
    );

    Task<BankAccountResponse> DepositAsync(
        Guid accountId,
        decimal amount,
        Guid actorUserId,
        string actorRole,
        CancellationToken ct = default
    );
    Task<BankAccountResponse> WithdrawAsync(
        Guid accountId,
        decimal amount,
        Guid actorUserId,
        string actorRole,
        CancellationToken ct = default
    );
    Task<(BankAccountResponse Sender, BankAccountResponse Receiver)> TransferAsync(
        Guid senderId,
        Guid receiverId,
        decimal amount,
        Guid actorUserId,
        string actorRole,
        CancellationToken ct = default
    );

    Task<IReadOnlyList<AccountTransactionResponse>> GetOwnTransactionsAsync(
        Guid appUserId,
        Guid accountId,
        CancellationToken ct = default
    );
    Task<IReadOnlyList<AccountTransactionResponse>> GetTransactionsAsync(
        Guid accountId,
        CancellationToken ct = default
    );
}
