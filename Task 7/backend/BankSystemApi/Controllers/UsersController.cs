using BankApi.Application.DTOs.Responses;
using BankApi.Application.Interfaces;
using BankApi.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankApi.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(AuthenticationSchemes = "AppJwt")]
public class UsersController : ControllerBase
{
    private readonly IUserDirectoryService _service;

    public UsersController(IUserDirectoryService service)
    {
        _service = service;
    }

    [Authorize(Policy = AppPolicies.AccountsAllRead)]
    [HttpGet("customers")]
    public async Task<
        ActionResult<ApiResponse<IReadOnlyList<CustomerLookupResponse>>>
    > SearchCustomers([FromQuery] string? search, CancellationToken ct)
    {
        var data = await _service.SearchCustomersAsync(search, ct);
        return Ok(ApiResponse<IReadOnlyList<CustomerLookupResponse>>.Ok(data));
    }
}
