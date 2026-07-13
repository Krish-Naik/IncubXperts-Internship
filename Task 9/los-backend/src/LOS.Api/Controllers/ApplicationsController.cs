using System.Security.Claims;
using LOS.Application.Applications;
using LOS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LOS.Api.Controllers;

[ApiController]
[Route("api/applications")]
[Authorize(Policy = "Customer")]
public class ApplicationsController(LoanApplicationService service) : ControllerBase
{
    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public record CreateDraftRequest(
        LoanType LoanType,
        decimal RequestedAmount,
        int RequestedTenureMonths,
        Dictionary<string, string>? ExtraDetails
    );

    public record RespondInfoRequest(string ResponseText);

    public record AddCoApplicantRequest(string FullName, string Pan, decimal MonthlyIncome);

    [HttpPost("draft")]
    public async Task<IActionResult> CreateDraft(
        [FromBody] CreateDraftRequest request,
        CancellationToken ct
    )
    {
        var app = await service.CreateDraftAsync(
            CurrentUserId,
            request.LoanType,
            request.RequestedAmount,
            request.RequestedTenureMonths,
            request.ExtraDetails,
            ct
        );
        return Ok(app);
    }

    [HttpPost("{id:guid}/submit")]
    public async Task<IActionResult> Submit(Guid id, CancellationToken ct)
    {
        var app = await service.SubmitAsync(id, CurrentUserId, ct);
        return Ok(app);
    }

    [HttpPost("{id:guid}/respond-info")]
    public async Task<IActionResult> RespondInfo(
        Guid id,
        [FromBody] RespondInfoRequest request,
        CancellationToken ct
    ) => Ok(await service.RespondToInfoRequestAsync(id, CurrentUserId, request.ResponseText, ct));

    [HttpPost("{id:guid}/co-applicants")]
    public async Task<IActionResult> AddCoApplicant(
        Guid id,
        [FromBody] AddCoApplicantRequest request,
        CancellationToken ct
    ) =>
        Ok(
            await service.AddCoApplicantAsync(
                id,
                CurrentUserId,
                request.FullName,
                request.Pan,
                request.MonthlyIncome,
                ct
            )
        );

    [HttpDelete("{id:guid}/co-applicants/{coApplicantId:guid}")]
    public async Task<IActionResult> RemoveCoApplicant(
        Guid id,
        Guid coApplicantId,
        CancellationToken ct
    )
    {
        await service.RemoveCoApplicantAsync(id, coApplicantId, CurrentUserId, ct);
        return NoContent();
    }

    [HttpGet("mine")]
    public async Task<IActionResult> GetMine(CancellationToken ct) =>
        Ok(await service.GetMyApplicationsAsync(CurrentUserId, ct));
}
