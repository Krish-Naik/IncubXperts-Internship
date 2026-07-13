using LOS.Domain.Entities;
using LOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LOS.Application.Reporting;

public class AuditService(LOSDbContext db)
{
    public async Task<List<ApplicationAuditLog>> GetAuditTrailAsync(
        Guid applicationId,
        CancellationToken ct
    ) =>
        await db
            .ApplicationAuditLogs.Where(l => l.LoanApplicationId == applicationId)
            .OrderBy(l => l.TimestampUtc)
            .ToListAsync(ct);
}
