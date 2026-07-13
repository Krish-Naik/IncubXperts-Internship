using System.Security.Claims;
using BankApi.Application.DTOs.Requests;
using BankApi.Application.DTOs.Responses;
using BankApi.Application.Interfaces;
using BankApi.Authorization;
using BankApi.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankApi.Controllers;

[ApiController]
[Route("api/accounts")]
[Authorize(AuthenticationSchemes = "AppJwt")]
public class AccountsController : ControllerBase
{
    private readonly IBankAccountService _service;

    public AccountsController(IBankAccountService service)
    {
        _service = service;
    }

    [Authorize(Policy = AppPolicies.AccountsOwnRead)]
    [HttpGet("my")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<BankAccountResponse>>>> GetMyAccounts(
        CancellationToken ct
    )
    {
        var data = await _service.GetOwnAccountsAsync(GetCurrentUserId(), ct);
        return Ok(ApiResponse<IReadOnlyList<BankAccountResponse>>.Ok(data));
    }

    [Authorize(Policy = AppPolicies.TransactionsOwnRead)]
    [HttpGet("my/{id:guid}/transactions")]
    public async Task<
        ActionResult<ApiResponse<IReadOnlyList<AccountTransactionResponse>>>
    > GetMyTransactions(Guid id, CancellationToken ct)
    {
        var data = await _service.GetOwnTransactionsAsync(GetCurrentUserId(), id, ct);
        return Ok(ApiResponse<IReadOnlyList<AccountTransactionResponse>>.Ok(data));
    }

    [Authorize(Policy = AppPolicies.AccountsOwnDeposit)]
    [HttpPut("my/{id:guid}/deposit")]
    public async Task<ActionResult<ApiResponse<BankAccountResponse>>> DepositOwn(
        Guid id,
        [FromBody] AmountRequest request,
        CancellationToken ct
    )
    {
        var data = await _service.DepositOwnAsync(GetCurrentUserId(), id, request.Amount, ct);
        return Ok(ApiResponse<BankAccountResponse>.Ok(data, "Money deposited successfully"));
    }

    [Authorize(Policy = AppPolicies.AccountsOwnWithdraw)]
    [HttpPut("my/{id:guid}/withdraw")]
    public async Task<ActionResult<ApiResponse<BankAccountResponse>>> WithdrawOwn(
        Guid id,
        [FromBody] AmountRequest request,
        CancellationToken ct
    )
    {
        var data = await _service.WithdrawOwnAsync(GetCurrentUserId(), id, request.Amount, ct);
        return Ok(ApiResponse<BankAccountResponse>.Ok(data, "Money withdrawn successfully"));
    }

    [Authorize(Policy = AppPolicies.AccountsAllRead)]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<BankAccountResponse>>>> Search(
        [FromQuery] string? search,
        CancellationToken ct
    )
    {
        var data = await _service.SearchAsync(search, ct);
        return Ok(ApiResponse<IReadOnlyList<BankAccountResponse>>.Ok(data));
    }

    [Authorize(Policy = AppPolicies.AccountsAllRead)]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<BankAccountResponse>>> GetById(
        Guid id,
        CancellationToken ct
    )
    {
        var data = await _service.GetByIdAsync(id, ct);
        return Ok(ApiResponse<BankAccountResponse>.Ok(data));
    }

    [Authorize(Policy = AppPolicies.TransactionsAllRead)]
    [HttpGet("{id:guid}/transactions")]
    public async Task<
        ActionResult<ApiResponse<IReadOnlyList<AccountTransactionResponse>>>
    > GetTransactions(Guid id, CancellationToken ct)
    {
        var data = await _service.GetTransactionsAsync(id, ct);
        return Ok(ApiResponse<IReadOnlyList<AccountTransactionResponse>>.Ok(data));
    }

    [Authorize(Policy = AppPolicies.AccountsCreate)]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<BankAccountResponse>>> Create(
        [FromBody] CreateAccountRequest request,
        CancellationToken ct
    )
    {
        var data = await _service.CreateAsync(request, GetCurrentUserId(), GetCurrentRole(), ct);
        return CreatedAtAction(
            nameof(GetById),
            new { id = data.Id },
            ApiResponse<BankAccountResponse>.Ok(data, "Account created successfully")
        );
    }

    [Authorize(Policy = AppPolicies.AccountsUpdate)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<BankAccountResponse>>> Update(
        Guid id,
        [FromBody] UpdateAccountRequest request,
        CancellationToken ct
    )
    {
        var data = await _service.UpdateAsync(
            id,
            request,
            GetCurrentUserId(),
            GetCurrentRole(),
            ct
        );
        return Ok(ApiResponse<BankAccountResponse>.Ok(data, "Account updated successfully"));
    }

    [Authorize(Policy = AppPolicies.AccountsClose)]
    [HttpPut("{id:guid}/close")]
    public async Task<ActionResult<ApiResponse<BankAccountResponse>>> Close(
        Guid id,
        CancellationToken ct
    )
    {
        var data = await _service.CloseAsync(id, GetCurrentUserId(), GetCurrentRole(), ct);
        return Ok(ApiResponse<BankAccountResponse>.Ok(data, "Account closed successfully"));
    }

    [Authorize(Policy = AppPolicies.AccountsCashDeposit)]
    [HttpPut("{id:guid}/deposit")]
    public async Task<ActionResult<ApiResponse<BankAccountResponse>>> Deposit(
        Guid id,
        [FromBody] AmountRequest request,
        CancellationToken ct
    )
    {
        var data = await _service.DepositAsync(
            id,
            request.Amount,
            GetCurrentUserId(),
            GetCurrentRole(),
            ct
        );
        return Ok(ApiResponse<BankAccountResponse>.Ok(data, "Money deposited successfully"));
    }

    [Authorize(Policy = AppPolicies.AccountsCashWithdraw)]
    [HttpPut("{id:guid}/withdraw")]
    public async Task<ActionResult<ApiResponse<BankAccountResponse>>> Withdraw(
        Guid id,
        [FromBody] AmountRequest request,
        CancellationToken ct
    )
    {
        var data = await _service.WithdrawAsync(
            id,
            request.Amount,
            GetCurrentUserId(),
            GetCurrentRole(),
            ct
        );
        return Ok(ApiResponse<BankAccountResponse>.Ok(data, "Money withdrawn successfully"));
    }

    [Authorize(Policy = AppPolicies.TransactionsTransfer)]
    [HttpPut("{id:guid}/transfer")]
    public async Task<ActionResult<ApiResponse<object>>> Transfer(
        Guid id,
        [FromBody] TransferRequest request,
        CancellationToken ct
    )
    {
        var result = await _service.TransferAsync(
            id,
            request.ReceiverId,
            request.Amount,
            GetCurrentUserId(),
            GetCurrentRole(),
            ct
        );
        return Ok(
            ApiResponse<object>.Ok(
                new { sender = result.Sender, receiver = result.Receiver },
                "Transfer successful"
            )
        );
    }

    private Guid GetCurrentUserId()
    {
        var raw =
            User.FindFirstValue(AppClaimTypes.InternalUserId)
            ?? throw new UnauthorizedAccessException("Missing user id claim.");
        return Guid.Parse(raw);
    }

    private string GetCurrentRole()
    {
        return User.FindFirstValue(ClaimTypes.Role) ?? "Unknown";
    }
}
