using BankApi.Application.DTOs.Responses;
using BankApi.Domain.Entities;

namespace BankApi.Application.Mappers;

public static class BankAccountMapper
{
    public static BankAccountResponse ToResponse(BankAccount account)
    {
        return new BankAccountResponse
        {
            Id = account.Id,
            AppUserId = account.AppUserId,
            AccountNumber = account.AccountNumber,
            HolderName = account.HolderName,
            Email = account.Email,
            Type = account.Type,
            Balance = account.Balance,
            IsActive = account.IsActive,
            CreatedAtUtc = account.CreatedAtUtc,
            UpdatedAtUtc = account.UpdatedAtUtc,
        };
    }
}
