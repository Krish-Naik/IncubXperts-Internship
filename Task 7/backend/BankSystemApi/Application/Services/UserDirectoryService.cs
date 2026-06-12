using BankApi.Application.DTOs.Responses;
using BankApi.Application.Interfaces;
using BankApi.Domain.Constants;
using BankApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BankApi.Application.Services;

public sealed class UserDirectoryService : IUserDirectoryService
{
    private readonly AppDbContext _db;

    public UserDirectoryService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<CustomerLookupResponse>> SearchCustomersAsync(
        string? search,
        CancellationToken ct = default
    )
    {
        var query = _db
            .AppUsers.AsNoTracking()
            .Where(x => x.Role == AppRoles.Customer && x.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(x =>
                x.DisplayName.ToLower().Contains(term) || x.Email.ToLower().Contains(term)
            );
        }

        return await query
            .OrderBy(x => x.DisplayName)
            .Take(25)
            .Select(x => new CustomerLookupResponse
            {
                Id = x.Id,
                DisplayName = x.DisplayName,
                Email = x.Email,
            })
            .ToListAsync(ct);
    }
}
