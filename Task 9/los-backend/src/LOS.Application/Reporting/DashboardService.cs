using LOS.Domain.Enums;
using LOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LOS.Application.Reporting;

public class DashboardService(LOSDbContext db)
{
    public record StatusCount(ApplicationStatus Status, int Count);

    public record DashboardSummary(
        List<StatusCount> StatusBreakdown,
        double AverageTurnaroundDays,
        int TotalApplications
    );

    public async Task<DashboardSummary> GetSummaryAsync(
        DateTime? from,
        DateTime? to,
        CancellationToken ct
    )
    {
        var query = db.LoanApplications.AsQueryable();
        if (from.HasValue)
            query = query.Where(a => a.CreatedAtUtc >= from.Value);
        if (to.HasValue)
            query = query.Where(a => a.CreatedAtUtc <= to.Value);

        var applications = await query.ToListAsync(ct);

        var statusBreakdown = applications
            .GroupBy(a => a.Status)
            .Select(g => new StatusCount(g.Key, g.Count()))
            .ToList();

        var decided = applications
            .Where(a =>
                a.Status is ApplicationStatus.Approved or ApplicationStatus.Rejected
                && a.SubmittedAtUtc.HasValue
            )
            .ToList();

        var avgDays =
            decided.Count > 0
                ? decided.Average(a => (a.UpdatedAtUtc - a.SubmittedAtUtc!.Value).TotalDays)
                : 0;

        return new DashboardSummary(statusBreakdown, Math.Round(avgDays, 1), applications.Count);
    }
}
