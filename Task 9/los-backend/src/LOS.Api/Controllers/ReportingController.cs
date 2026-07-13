using LOS.Application.Reporting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LOS.Api.Controllers;

[ApiController]
[Route("api/reporting")]
public class ReportingController(
    AuditService auditService,
    DashboardService dashboardService,
    ReportExportService exportService
) : ControllerBase
{
    [HttpGet("audit/{applicationId:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetAuditTrail(Guid applicationId, CancellationToken ct) =>
        Ok(await auditService.GetAuditTrailAsync(applicationId, ct));

    [HttpGet("dashboard")]
    [Authorize(Policy = "BranchManager")]
    public async Task<IActionResult> GetDashboard(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken ct
    ) => Ok(await dashboardService.GetSummaryAsync(from, to, ct));

    [HttpGet("export")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Export(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken ct
    )
    {
        var bytes = await exportService.ExportApplicationsCsvAsync(from, to, maskPan: true, ct);
        return File(bytes, "text/csv", $"los-report-{DateTime.UtcNow:yyyyMMdd}.csv");
    }
}
