using BankApi.Application.DTOs.Requests;
using BankApi.Application.DTOs.Responses;
using BankApi.Application.Exceptions;
using BankApi.Application.Interfaces;
using BankApi.Application.Mappers;
using BankApi.Domain.Entities;
using BankApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BankApi.Application.Services;

public class BankAccountService : IBankAccountService
{
    private readonly AppDbContext _db;

    public BankAccountService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<BankAccountResponse>> GetOwnAccountsAsync(
        Guid appUserId,
        CancellationToken ct = default
    )
    {
        var accounts = await _db
            .BankAccounts.AsNoTracking()
            .Where(x => x.AppUserId == appUserId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(ct);

        return accounts.Select(BankAccountMapper.ToResponse).ToList();
    }

    public async Task<IReadOnlyList<BankAccountResponse>> SearchAsync(
        string? search,
        CancellationToken ct = default
    )
    {
        var query = _db.BankAccounts.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(x =>
                x.AccountNumber.ToLower().Contains(term)
                || x.HolderName.ToLower().Contains(term)
                || x.Email.ToLower().Contains(term)
            );
        }

        var accounts = await query.OrderByDescending(x => x.CreatedAtUtc).Take(100).ToListAsync(ct);

        return accounts.Select(BankAccountMapper.ToResponse).ToList();
    }

    public async Task<BankAccountResponse> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var account = await GetEntityOrThrow(id, ct);
        return BankAccountMapper.ToResponse(account);
    }

    public async Task<BankAccountResponse> CreateAsync(
        CreateAccountRequest request,
        Guid actorUserId,
        string actorRole,
        CancellationToken ct = default
    )
    {
        var owner = await _db.AppUsers.FirstOrDefaultAsync(
            x => x.Id == request.AppUserId && x.IsActive,
            ct
        );
        if (owner is null)
            throw new BusinessException("Customer not found", 404);

        var normalizedType = NormalizeType(request.Type);
        var typeAlreadyExists = await _db.BankAccounts.AnyAsync(
            x => x.AppUserId == owner.Id && x.Type == normalizedType && x.IsActive,
            ct
        );

        if (typeAlreadyExists)
            throw new BusinessException(
                $"Customer already has an active {normalizedType} account. "
                    + "Close the existing one before opening another of the same type.",
                409
            );

        var account = new BankAccount
        {
            Id = Guid.NewGuid(),
            AppUserId = owner.Id,
            AccountNumber = GenerateAccountNumber(),
            HolderName = owner.DisplayName.Trim(),
            Email = owner.Email.Trim().ToLowerInvariant(),
            Type = normalizedType,
            Balance = request.OpeningBalance,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow,
        };

        _db.BankAccounts.Add(account);

        _db.AccountTransactions.Add(
            new AccountTransaction
            {
                Id = Guid.NewGuid(),
                BankAccountId = account.Id,
                Type = "ACCOUNT_CREATED",
                Amount = request.OpeningBalance,
                BalanceAfter = request.OpeningBalance,
                Description = "Account created",
                PerformedByUserId = actorUserId,
                PerformedByRole = actorRole,
                CreatedAtUtc = DateTime.UtcNow,
            }
        );

        await _db.SaveChangesAsync(ct);
        return BankAccountMapper.ToResponse(account);
    }

    public async Task<BankAccountResponse> UpdateAsync(
        Guid id,
        UpdateAccountRequest request,
        Guid actorUserId,
        string actorRole,
        CancellationToken ct = default
    )
    {
        var account = await GetEntityOrThrow(id, ct);
        var normalizedType = NormalizeType(request.Type);

        await EnsureUniqueEmailAsync(request.Email, id, ct);

        account.HolderName = request.HolderName.Trim();
        account.Email = request.Email.Trim().ToLowerInvariant();
        account.Type = normalizedType;
        account.IsActive = request.IsActive;
        account.UpdatedAtUtc = DateTime.UtcNow;

        _db.AccountTransactions.Add(
            new AccountTransaction
            {
                Id = Guid.NewGuid(),
                BankAccountId = account.Id,
                Type = "ACCOUNT_UPDATED",
                Amount = 0,
                BalanceAfter = account.Balance,
                Description = "Account details updated",
                PerformedByUserId = actorUserId,
                PerformedByRole = actorRole,
                CreatedAtUtc = DateTime.UtcNow,
            }
        );

        await _db.SaveChangesAsync(ct);
        return BankAccountMapper.ToResponse(account);
    }

    public async Task<BankAccountResponse> CloseAsync(
        Guid id,
        Guid actorUserId,
        string actorRole,
        CancellationToken ct = default
    )
    {
        var account = await GetEntityOrThrow(id, ct);
        account.IsActive = false;
        account.UpdatedAtUtc = DateTime.UtcNow;

        _db.AccountTransactions.Add(
            new AccountTransaction
            {
                Id = Guid.NewGuid(),
                BankAccountId = account.Id,
                Type = "ACCOUNT_CLOSED",
                Amount = 0,
                BalanceAfter = account.Balance,
                Description = "Account closed",
                PerformedByUserId = actorUserId,
                PerformedByRole = actorRole,
                CreatedAtUtc = DateTime.UtcNow,
            }
        );

        await _db.SaveChangesAsync(ct);
        return BankAccountMapper.ToResponse(account);
    }

    public async Task<BankAccountResponse> DepositOwnAsync(
        Guid appUserId,
        Guid accountId,
        decimal amount,
        CancellationToken ct = default
    )
    {
        ValidateAmount(amount);

        var account = await GetEntityOrThrow(accountId, ct);
        EnsureOwner(account, appUserId);
        EnsureActive(account);

        account.Balance += amount;
        account.UpdatedAtUtc = DateTime.UtcNow;

        _db.AccountTransactions.Add(
            new AccountTransaction
            {
                Id = Guid.NewGuid(),
                BankAccountId = account.Id,
                Type = "DEPOSIT",
                Amount = amount,
                BalanceAfter = account.Balance,
                Description = "Customer self deposit",
                PerformedByUserId = appUserId,
                PerformedByRole = "Customer",
                CreatedAtUtc = DateTime.UtcNow,
            }
        );

        await _db.SaveChangesAsync(ct);
        return BankAccountMapper.ToResponse(account);
    }

    public async Task<BankAccountResponse> WithdrawOwnAsync(
        Guid appUserId,
        Guid accountId,
        decimal amount,
        CancellationToken ct = default
    )
    {
        ValidateAmount(amount);

        var account = await GetEntityOrThrow(accountId, ct);
        EnsureOwner(account, appUserId);
        EnsureActive(account);

        if (account.Balance < amount)
            throw new BusinessException("Insufficient balance", 400);

        account.Balance -= amount;
        account.UpdatedAtUtc = DateTime.UtcNow;

        _db.AccountTransactions.Add(
            new AccountTransaction
            {
                Id = Guid.NewGuid(),
                BankAccountId = account.Id,
                Type = "WITHDRAW",
                Amount = amount,
                BalanceAfter = account.Balance,
                Description = "Customer self withdrawal",
                PerformedByUserId = appUserId,
                PerformedByRole = "Customer",
                CreatedAtUtc = DateTime.UtcNow,
            }
        );

        await _db.SaveChangesAsync(ct);
        return BankAccountMapper.ToResponse(account);
    }

    public async Task<BankAccountResponse> DepositAsync(
        Guid accountId,
        decimal amount,
        Guid actorUserId,
        string actorRole,
        CancellationToken ct = default
    )
    {
        ValidateAmount(amount);

        var account = await GetEntityOrThrow(accountId, ct);
        EnsureActive(account);

        account.Balance += amount;
        account.UpdatedAtUtc = DateTime.UtcNow;

        _db.AccountTransactions.Add(
            new AccountTransaction
            {
                Id = Guid.NewGuid(),
                BankAccountId = account.Id,
                Type = "DEPOSIT",
                Amount = amount,
                BalanceAfter = account.Balance,
                Description = "Staff deposit",
                PerformedByUserId = actorUserId,
                PerformedByRole = actorRole,
                CreatedAtUtc = DateTime.UtcNow,
            }
        );

        await _db.SaveChangesAsync(ct);
        return BankAccountMapper.ToResponse(account);
    }

    public async Task<BankAccountResponse> WithdrawAsync(
        Guid accountId,
        decimal amount,
        Guid actorUserId,
        string actorRole,
        CancellationToken ct = default
    )
    {
        ValidateAmount(amount);

        var account = await GetEntityOrThrow(accountId, ct);
        EnsureActive(account);

        if (account.Balance < amount)
            throw new BusinessException("Insufficient balance", 400);

        account.Balance -= amount;
        account.UpdatedAtUtc = DateTime.UtcNow;

        _db.AccountTransactions.Add(
            new AccountTransaction
            {
                Id = Guid.NewGuid(),
                BankAccountId = account.Id,
                Type = "WITHDRAW",
                Amount = amount,
                BalanceAfter = account.Balance,
                Description = "Staff withdrawal",
                PerformedByUserId = actorUserId,
                PerformedByRole = actorRole,
                CreatedAtUtc = DateTime.UtcNow,
            }
        );

        await _db.SaveChangesAsync(ct);
        return BankAccountMapper.ToResponse(account);
    }

    public async Task<(BankAccountResponse Sender, BankAccountResponse Receiver)> TransferAsync(
        Guid senderId,
        Guid receiverId,
        decimal amount,
        Guid actorUserId,
        string actorRole,
        CancellationToken ct = default
    )
    {
        ValidateAmount(amount);

        if (senderId == receiverId)
            throw new BusinessException("Sender and receiver cannot be the same account", 400);

        var sender = await GetEntityOrThrow(senderId, ct);
        var receiver = await GetEntityOrThrow(receiverId, ct);

        EnsureActive(sender);
        EnsureActive(receiver);

        if (sender.Balance < amount)
            throw new BusinessException("Insufficient balance", 400);

        sender.Balance -= amount;
        receiver.Balance += amount;
        sender.UpdatedAtUtc = DateTime.UtcNow;
        receiver.UpdatedAtUtc = DateTime.UtcNow;

        _db.AccountTransactions.Add(
            new AccountTransaction
            {
                Id = Guid.NewGuid(),
                BankAccountId = sender.Id,
                RelatedAccountId = receiver.Id,
                Type = "TRANSFER_OUT",
                Amount = amount,
                BalanceAfter = sender.Balance,
                Description = $"Transferred to {receiver.AccountNumber}",
                PerformedByUserId = actorUserId,
                PerformedByRole = actorRole,
                CreatedAtUtc = DateTime.UtcNow,
            }
        );

        _db.AccountTransactions.Add(
            new AccountTransaction
            {
                Id = Guid.NewGuid(),
                BankAccountId = receiver.Id,
                RelatedAccountId = sender.Id,
                Type = "TRANSFER_IN",
                Amount = amount,
                BalanceAfter = receiver.Balance,
                Description = $"Received from {sender.AccountNumber}",
                PerformedByUserId = actorUserId,
                PerformedByRole = actorRole,
                CreatedAtUtc = DateTime.UtcNow,
            }
        );

        await _db.SaveChangesAsync(ct);

        return (BankAccountMapper.ToResponse(sender), BankAccountMapper.ToResponse(receiver));
    }

    public async Task<IReadOnlyList<AccountTransactionResponse>> GetOwnTransactionsAsync(
        Guid appUserId,
        Guid accountId,
        CancellationToken ct = default
    )
    {
        var account = await GetEntityOrThrow(accountId, ct);
        EnsureOwner(account, appUserId);

        return await _db
            .AccountTransactions.AsNoTracking()
            .Where(x => x.BankAccountId == accountId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(ToTransactionResponse())
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<AccountTransactionResponse>> GetTransactionsAsync(
        Guid accountId,
        CancellationToken ct = default
    )
    {
        await GetEntityOrThrow(accountId, ct);

        return await _db
            .AccountTransactions.AsNoTracking()
            .Where(x => x.BankAccountId == accountId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(ToTransactionResponse())
            .ToListAsync(ct);
    }

    private static System.Linq.Expressions.Expression<
        Func<AccountTransaction, AccountTransactionResponse>
    > ToTransactionResponse() =>
        x => new AccountTransactionResponse
        {
            Id = x.Id,
            BankAccountId = x.BankAccountId,
            RelatedAccountId = x.RelatedAccountId,
            Type = x.Type,
            Amount = x.Amount,
            BalanceAfter = x.BalanceAfter,
            Description = x.Description,
            PerformedByRole = x.PerformedByRole,
            CreatedAtUtc = x.CreatedAtUtc,
        };

    private async Task<BankAccount> GetEntityOrThrow(Guid id, CancellationToken ct)
    {
        var account = await _db.BankAccounts.FirstOrDefaultAsync(x => x.Id == id, ct);

        if (account is null)
            throw new BusinessException("Account not found", 404);

        return account;
    }

    private async Task EnsureUniqueEmailAsync(string email, Guid? ignoreId, CancellationToken ct)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        var exists = await _db.BankAccounts.AnyAsync(
            x => x.Email == normalizedEmail && (!ignoreId.HasValue || x.Id != ignoreId.Value),
            ct
        );

        if (exists)
            throw new BusinessException("Email already exists", 409);
    }

    private static void EnsureOwner(BankAccount account, Guid appUserId)
    {
        if (account.AppUserId != appUserId)
            throw new BusinessException("You can only access your own account", 403);
    }

    private static void ValidateAmount(decimal amount)
    {
        if (amount <= 0)
            throw new BusinessException("Amount must be greater than zero", 400);
    }

    private static void EnsureActive(BankAccount account)
    {
        if (!account.IsActive)
            throw new BusinessException("Inactive accounts cannot perform transactions", 400);
    }

    private static string NormalizeType(string type)
    {
        var normalized = type.Trim().ToUpperInvariant();

        if (normalized is not ("SAVINGS" or "CURRENT"))
            throw new BusinessException("Type must be SAVINGS or CURRENT", 400);

        return normalized;
    }

    private static string GenerateAccountNumber()
    {
        return $"BNK-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}";
    }
}
