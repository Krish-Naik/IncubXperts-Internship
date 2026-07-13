using LOS.Api.Contracts.Requests;
using LOS.Application.Users;
using LOS.Application.Users.DTOs;
using LOS.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LOS.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Policy = "AdminOnly")]
public class UsersController(UserService userService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserListItemDto>>> GetUsers(
        CancellationToken ct
    ) => Ok(await userService.GetUsersAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserDetailDto>> GetUser(Guid id, CancellationToken ct) =>
        Ok(await userService.GetByIdAsync(id, ct));

    [HttpPost]
    public async Task<ActionResult<UserDetailDto>> CreateUser(
        [FromBody] CreateUserRequest request,
        CancellationToken ct
    )
    {
        var user = await userService.CreateAsync(
            new CreateUserDto(
                request.EmployeeId,
                request.FullName,
                request.Email,
                request.Phone,
                request.RoleId,
                request.BranchId
            ),
            ct
        );
        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UserDetailDto>> UpdateUser(
        Guid id,
        [FromBody] UpdateUserRequest request,
        CancellationToken ct
    ) =>
        Ok(
            await userService.UpdateAsync(
                id,
                new UpdateUserDto(
                    request.FullName,
                    request.Phone,
                    request.RoleId,
                    request.BranchId
                ),
                ct
            )
        );

    [HttpPost("{id:guid}/activate")]
    public async Task<ActionResult<UserDetailDto>> Activate(Guid id, CancellationToken ct) =>
        Ok(await userService.ActivateAsync(id, ct));

    [HttpPost("{id:guid}/deactivate")]
    public async Task<ActionResult<UserDetailDto>> Deactivate(Guid id, CancellationToken ct) =>
        Ok(await userService.DeactivateAsync(id, ct));

    [HttpPut("{id:guid}/role")]
    public async Task<ActionResult<UserDetailDto>> AssignRole(
        Guid id,
        [FromBody] AssignRoleRequest request,
        CancellationToken ct
    ) => Ok(await userService.AssignRoleAsync(id, request.RoleId, ct));

    [HttpPut("{id:guid}/branch")]
    public async Task<ActionResult<UserDetailDto>> AssignBranch(
        Guid id,
        [FromBody] AssignBranchRequest request,
        CancellationToken ct
    ) => Ok(await userService.AssignBranchAsync(id, request.BranchId, ct));

    [HttpGet("{id:guid}/invite-status")]
    public async Task<ActionResult<InviteStatusDto>> InviteStatus(Guid id, CancellationToken ct) =>
        Ok(await userService.GetInviteStatusAsync(id, ct));

    [HttpPost("{id:guid}/resend-invite")]
    public async Task<IActionResult> ResendInvite(Guid id, CancellationToken ct)
    {
        await userService.ResendInviteAsync(id, ct);
        return NoContent();
    }
}
