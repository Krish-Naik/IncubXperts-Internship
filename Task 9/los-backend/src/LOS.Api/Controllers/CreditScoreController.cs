using LOS.Application.CreditScore;
using LOS.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LOS.Api.Controllers;

[ApiController]
[Route("api/credit-score")]
[Authorize(Policy = "BranchManager")]
public class CreditScoreController(ICreditScoreProvider creditScoreProvider, LOSDbContext db)
    : ControllerBase
{
    public record CreditScoreResponse(
        int Score,
        string RiskCategory,
        bool IsSimulated,
        string Disclaimer
    );

    [HttpGet("{applicationId:guid}")]
    public async Task<IActionResult> GetScore(
        Guid applicationId,
        [FromQuery] string pan,
        [FromQuery] DateOnly dob,
        CancellationToken ct
    )
    {
        var app =
            await db.LoanApplications.FirstOrDefaultAsync(a => a.Id == applicationId, ct)
            ?? throw new InvalidOperationException("Application not found.");

        var result = creditScoreProvider.GetScore(pan, dob);

        return Ok(
            new CreditScoreResponse(
                result.Score,
                result.RiskCategory,
                result.IsSimulated,
                "Demo Mode – Simulated Score, Not a Real Credit Bureau"
            )
        );
    }
}
