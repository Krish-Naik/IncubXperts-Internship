using System.Security.Claims;
using LOS.Application.Brokers;
using LOS.Application.Kyc;
using LOS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LOS.Api.Controllers;

[ApiController]
[Route("api/broker")]
[Authorize(Policy = "BrokerAgent")]
public class BrokerController(BrokerService brokerService, KycService kycService) : ControllerBase
{
    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public record RegisterLeadRequest(string FullName, string Email, string Phone);

    [HttpPost("leads")]
    public async Task<IActionResult> RegisterLead(
        [FromBody] RegisterLeadRequest req,
        CancellationToken ct
    ) =>
        Ok(
            await brokerService.RegisterLeadAsync(
                CurrentUserId,
                req.FullName,
                req.Email,
                req.Phone,
                ct
            )
        );

    [HttpGet("leads")]
    public async Task<IActionResult> GetMyLeads(CancellationToken ct) =>
        Ok(await brokerService.GetMyLeadsAsync(CurrentUserId, ct));

    [HttpPost("leads/{leadId:guid}/resend-invite")]
    public async Task<IActionResult> ResendLeadInvite(Guid leadId, CancellationToken ct)
    {
        await brokerService.ResendLeadInviteAsync(CurrentUserId, leadId, ct);
        return NoContent();
    }

    [HttpGet("pipeline")]
    public async Task<IActionResult> GetPipeline(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken ct
    ) => Ok(await brokerService.GetPipelineAsync(CurrentUserId, from, to, ct));

    [HttpPost("{applicationId:guid}/upload-document")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> UploadOnBehalf(
        Guid applicationId,
        [FromForm] DocumentType docType,
        IFormFile file,
        CancellationToken ct
    )
    {
        await brokerService.EnsureBrokerOwnsApplicationAsync(CurrentUserId, applicationId, ct);

        await using var stream = file.OpenReadStream();
        var doc = await kycService.UploadAsync(
            applicationId,
            docType,
            stream,
            file.FileName,
            file.Length,
            true,
            ct
        );
        return Ok(doc);
    }
}
