using LOS.Application.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LOS.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Policy = "AdminOnly")]
public class AdminController(BranchPolicyService policyService, BrandingService brandingService)
    : ControllerBase
{
    public record PolicyRequest(decimal MinRate, decimal MaxRate, decimal HighValueThreshold);

    [HttpPut("branches/{branchId:guid}/policy")]
    public async Task<IActionResult> SetPolicy(
        Guid branchId,
        [FromBody] PolicyRequest req,
        CancellationToken ct
    ) =>
        Ok(
            await policyService.UpsertAsync(
                branchId,
                req.MinRate,
                req.MaxRate,
                req.HighValueThreshold,
                ct
            )
        );

    [HttpPost("branding/logo")]
    [RequestSizeLimit(1 * 1024 * 1024)]
    public async Task<IActionResult> UpdateLogo(IFormFile file, CancellationToken ct)
    {
        await using var stream = file.OpenReadStream();
        return Ok(await brandingService.UpdateLogoAsync(stream, file.FileName, file.Length, ct));
    }
}
