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

    public async Task<IReadOnlyList<BankAccountResponse>> GetAllAsync(CancellationToken ct = default)
    {
        var accounts = await _db.BankAccounts
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(ct);

        return accounts.Select(BankAccountMapper.ToResponse).ToList();
    }

    public async Task<BankAccountResponse> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var account = await GetEntityOrThrow(id, ct);
        return BankAccountMapper.ToResponse(account);
    }

    public async Task<BankAccountResponse> CreateAsync(CreateAccountRequest request, CancellationToken ct = default)
    {
        var normalizedType = NormalizeType(request.Type);
        await EnsureUniqueEmailAsync(request.Email, null, ct);

        var account = new BankAccount
        {
            Id = Guid.NewGuid(),
            AccountNumber = GenerateAccountNumber(),
            HolderName = request.HolderName.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            Type = normalizedType,
            Balance = request.OpeningBalance,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.BankAccounts.Add(account);
        await _db.SaveChangesAsync(ct);

        return BankAccountMapper.ToResponse(account);
    }

    public async Task<BankAccountResponse> UpdateAsync(Guid id, UpdateAccountRequest request, CancellationToken ct = default)
    {
        var account = await GetEntityOrThrow(id, ct);
        var normalizedType = NormalizeType(request.Type);

        await EnsureUniqueEmailAsync(request.Email, id, ct);

        account.HolderName = request.HolderName.Trim();
        account.Email = request.Email.Trim().ToLowerInvariant();
        account.Type = normalizedType;
        account.IsActive = request.IsActive;
        account.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        return BankAccountMapper.ToResponse(account);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var account = await GetEntityOrThrow(id, ct);
        _db.BankAccounts.Remove(account);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<BankAccountResponse> DepositAsync(Guid id, decimal amount, CancellationToken ct = default)
    {
        ValidateAmount(amount);

        var account = await GetEntityOrThrow(id, ct);
        EnsureActive(account);

        account.Balance += amount;
        account.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return BankAccountMapper.ToResponse(account);
    }

    public async Task<BankAccountResponse> WithdrawAsync(Guid id, decimal amount, CancellationToken ct = default)
    {
        ValidateAmount(amount);

        var account = await GetEntityOrThrow(id, ct);
        EnsureActive(account);

        if (account.Balance < amount)
            throw new BusinessException("Insufficient balance", 400);

        account.Balance -= amount;
        account.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return BankAccountMapper.ToResponse(account);
    }

    public async Task<(BankAccountResponse Sender, BankAccountResponse Receiver)> TransferAsync(Guid senderId, Guid receiverId, decimal amount, CancellationToken ct = default)
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

        await _db.SaveChangesAsync(ct);

        return (BankAccountMapper.ToResponse(sender), BankAccountMapper.ToResponse(receiver));
    }

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
            x => x.Email == normalizedEmail && (!ignoreId.HasValue || x.Id != ignoreId.Value), ct);

        if (exists)
            throw new BusinessException("Email already exists", 409);
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