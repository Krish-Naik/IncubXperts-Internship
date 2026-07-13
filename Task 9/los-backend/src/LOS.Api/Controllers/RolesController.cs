using LOS.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LOS.Api.Controllers;

[ApiController]
[Route("api/roles")]
[Authorize]
public class RolesController(LOSDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetRoles(CancellationToken ct) =>
        Ok(
            await db
                .Roles.OrderBy(x => x.Name)
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Description,
                })
                .ToListAsync(ct)
        );
}
