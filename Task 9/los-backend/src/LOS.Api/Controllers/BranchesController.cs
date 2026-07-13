using LOS.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LOS.Api.Controllers;

[ApiController]
[Route("api/branches")]
[Authorize]
public class BranchesController(LOSDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetBranches(CancellationToken ct) =>
        Ok(
            await db
                .Branches.Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .Select(x => new
                {
                    x.Id,
                    x.Code,
                    x.Name,
                    x.City,
                })
                .ToListAsync(ct)
        );
}
