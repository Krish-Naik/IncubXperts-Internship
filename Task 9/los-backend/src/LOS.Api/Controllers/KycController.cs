using LOS.Application.Applications;
using LOS.Application.Kyc;
using LOS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LOS.Api.Controllers;

[ApiController]
[Route("api/kyc")]
public class KycController(KycService kycService, LoanApplicationService applicationService)
    : ControllerBase
{
    [HttpPost("{applicationId:guid}/upload")]
    [Authorize(Policy = "Customer")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> Upload(
        Guid applicationId,
        [FromForm] DocumentType docType,
        IFormFile file,
        CancellationToken ct
    )
    {
        await using var stream = file.OpenReadStream();
        var doc = await kycService.UploadAsync(
            applicationId,
            docType,
            stream,
            file.FileName,
            file.Length,
            false,
            ct
        );
        return Ok(doc);
    }

    [HttpGet("queue")]
    [Authorize(Policy = "VerificationOfficer")]
    public async Task<IActionResult> GetQueue(CancellationToken ct) =>
        Ok(await kycService.GetReviewQueueAsync(ct));

    [HttpGet("applications/{applicationId:guid}/detail")]
    [Authorize(Policy = "VerificationOfficer")]
    public async Task<IActionResult> GetApplicationDetail(
        Guid applicationId,
        CancellationToken ct
    ) => Ok(await applicationService.GetApplicationDetailAsync(applicationId, ct));

    [HttpGet("{documentId:guid}/file")]
    [Authorize(Policy = "VerificationOfficer")]
    public async Task<IActionResult> GetFile(Guid documentId, CancellationToken ct)
    {
        var file = await kycService.GetDocumentFileAsync(documentId, ct);

        Response.Headers.ContentDisposition = $"inline; filename=\"{file.FileName}\"";
        return File(file.Content, file.ContentType);
    }

    public record ReviewRequest(DocumentStatus Status, string? Remarks);

    [HttpPost("{documentId:guid}/review")]
    [Authorize(Policy = "VerificationOfficer")]
    public async Task<IActionResult> Review(
        Guid documentId,
        [FromBody] ReviewRequest request,
        CancellationToken ct
    )
    {
        var officerId = Guid.Parse(
            User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value
        );
        await kycService.ReviewAsync(documentId, officerId, request.Status, request.Remarks, ct);
        return NoContent();
    }
}
