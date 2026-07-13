using System.Security.Claims;
using LOS.Application.Disbursement;
using LOS.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LOS.Api.Controllers;

[ApiController]
[Route("api/disbursement")]
public class DisbursementController(DisbursementService service, LOSDbContext db) : ControllerBase
{
    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("{id:guid}/accept-offer")]
    [Authorize(Policy = "Customer")]
    public async Task<IActionResult> AcceptOffer(Guid id, CancellationToken ct) =>
        Ok(await service.AcceptOfferAsync(id, CurrentUserId, ct));

    public record DisburseRequest(string BankReference);

    [HttpGet("queue")]
    [Authorize(Policy = "BranchManager")]
    public async Task<IActionResult> GetQueue(CancellationToken ct) =>
        Ok(await service.GetQueueAsync(ct));

    [HttpPost("{id:guid}/disburse")]
    [Authorize(Policy = "BranchManager")]
    public async Task<IActionResult> Disburse(
        Guid id,
        [FromBody] DisburseRequest req,
        CancellationToken ct
    ) => Ok(await service.DisburseAsync(id, CurrentUserId, req.BankReference, ct));

    [HttpGet("{id:guid}/schedule")]
    [Authorize(Policy = "Customer")]
    public async Task<IActionResult> GetSchedule(Guid id, CancellationToken ct)
    {
        var app =
            await db.LoanApplications.FirstOrDefaultAsync(
                a => a.Id == id && a.CustomerId == CurrentUserId,
                ct
            ) ?? throw new InvalidOperationException("Application not found.");
        return Ok(service.GetRepaymentSchedule(app));
    }
}
