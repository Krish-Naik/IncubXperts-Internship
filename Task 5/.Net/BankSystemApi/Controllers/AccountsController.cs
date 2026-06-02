using BankApi.Application.DTOs.Requests;
using BankApi.Application.DTOs.Responses;
using BankApi.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BankApi.Controllers;

[ApiController]
[Route("api/accounts")]
public class AccountsController : ControllerBase
{
    private readonly IBankAccountService _service;

    public AccountsController(IBankAccountService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<BankAccountResponse>>>> GetAll(CancellationToken ct)
    {
        var data = await _service.GetAllAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<BankAccountResponse>>.Ok(data));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<BankAccountResponse>>> GetById(Guid id, CancellationToken ct)
    {
        var data = await _service.GetByIdAsync(id, ct);
        return Ok(ApiResponse<BankAccountResponse>.Ok(data));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<BankAccountResponse>>> Create([FromBody] CreateAccountRequest request, CancellationToken ct)
    {
        var data = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = data.Id }, ApiResponse<BankAccountResponse>.Ok(data, "Account created successfully"));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<BankAccountResponse>>> Update(Guid id, [FromBody] UpdateAccountRequest request, CancellationToken ct)
    {
        var data = await _service.UpdateAsync(id, request, ct);
        return Ok(ApiResponse<BankAccountResponse>.Ok(data, "Account updated successfully"));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return Ok(ApiResponse<object>.Ok(null, "Account deleted successfully"));
    }

    [HttpPut("{id:guid}/deposit")]
    public async Task<ActionResult<ApiResponse<BankAccountResponse>>> Deposit(Guid id, [FromBody] AmountRequest request, CancellationToken ct)
    {
        var data = await _service.DepositAsync(id, request.Amount, ct);
        return Ok(ApiResponse<BankAccountResponse>.Ok(data, "Money deposited successfully"));
    }

    [HttpPut("{id:guid}/withdraw")]
    public async Task<ActionResult<ApiResponse<BankAccountResponse>>> Withdraw(Guid id, [FromBody] AmountRequest request, CancellationToken ct)
    {
        var data = await _service.WithdrawAsync(id, request.Amount, ct);
        return Ok(ApiResponse<BankAccountResponse>.Ok(data, "Money withdrawn successfully"));
    }

    [HttpPut("{id:guid}/transfer")]
    public async Task<ActionResult<ApiResponse<object>>> Transfer(Guid id, [FromBody] TransferRequest request, CancellationToken ct)
    {
        var result = await _service.TransferAsync(id, request.ReceiverId, request.Amount, ct);
        return Ok(ApiResponse<object>.Ok(new { sender = result.Sender, receiver = result.Receiver }, "Transfer successful"));
    }
}