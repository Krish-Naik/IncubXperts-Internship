using System.Security.Claims;
using LOS.Application.Applications;
using LOS.Application.Approvals;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LOS.Api.Controllers;

[ApiController]
[Route("api/approvals")]
[Authorize(Policy = "BranchManager")]
public class ApprovalsController(
    LoanApprovalService approvalService,
    LoanApplicationService applicationService
) : ControllerBase
{
    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public record ApproveRequest(decimal InterestRate, int TenureMonths);

    public record RejectRequest(string Reason);

    public record InfoRequest(string Details);

    public record SeniorDecisionRequest(bool Approve, string? Reason);

    [HttpGet("queue")]
    public async Task<IActionResult> GetQueue(CancellationToken ct) =>
        Ok(await approvalService.GetQueueAsync(ct));

    [HttpGet("applications/{applicationId:guid}/detail")]
    public async Task<IActionResult> GetApplicationDetail(
        Guid applicationId,
        CancellationToken ct
    ) => Ok(await applicationService.GetApplicationDetailAsync(applicationId, ct));

    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(
        Guid id,
        [FromBody] ApproveRequest req,
        CancellationToken ct
    ) =>
        Ok(
            await approvalService.ApproveAsync(
                id,
                CurrentUserId,
                req.InterestRate,
                req.TenureMonths,
                ct
            )
        );

    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(
        Guid id,
        [FromBody] RejectRequest req,
        CancellationToken ct
    ) => Ok(await approvalService.RejectAsync(id, CurrentUserId, req.Reason, ct));

    [HttpPost("{id:guid}/request-info")]
    public async Task<IActionResult> RequestInfo(
        Guid id,
        [FromBody] InfoRequest req,
        CancellationToken ct
    ) => Ok(await approvalService.RequestMoreInfoAsync(id, CurrentUserId, req.Details, ct));

    [HttpPost("{id:guid}/senior-decision")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> SeniorDecision(
        Guid id,
        [FromBody] SeniorDecisionRequest req,
        CancellationToken ct
    ) =>
        Ok(
            await approvalService.SeniorApproveAsync(id, CurrentUserId, req.Approve, req.Reason, ct)
        );
}
