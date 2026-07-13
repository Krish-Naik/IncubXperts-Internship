using System.Text;
using LOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LOS.Application.Reporting;

public class ReportExportService(LOSDbContext db)
{
    public async Task<byte[]> ExportApplicationsCsvAsync(
        DateTime? from,
        DateTime? to,
        bool maskPan,
        CancellationToken ct
    )
    {
        var query = db.LoanApplications.Include(a => a.Customer).AsQueryable();
        if (from.HasValue)
            query = query.Where(a => a.CreatedAtUtc >= from.Value);
        if (to.HasValue)
            query = query.Where(a => a.CreatedAtUtc <= to.Value);

        var rows = await query.ToListAsync(ct);

        var sb = new StringBuilder();
        sb.AppendLine(
            "ReferenceNumber,CustomerName,LoanType,Amount,Status,CreatedAtUtc,DemoModeDisclaimer"
        );

        foreach (var app in rows)
        {
            sb.AppendLine(
                $"{app.ReferenceNumber},{app.Customer.FullName},{app.LoanType},{app.RequestedAmount},{app.Status},{app.CreatedAtUtc:O},Simulated Credit Score - Not Real Bureau Data"
            );
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }
}
