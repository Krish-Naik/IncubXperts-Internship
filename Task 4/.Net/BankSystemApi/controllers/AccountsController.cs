using Microsoft.AspNetCore.Mvc;

using BankApi.Models;
using BankApi.Data;
using BankApi.Utils;

namespace BankApi.Controllers
{
    [ApiController]

    [Route("api/[controller]")]
    public class AccountsController : ControllerBase
    {

        // GET ALL ACCOUNTS
        [HttpGet]
        public ActionResult GetAccounts()
        {
            return Ok(new
            {
                success = true,
                data = AccountData.Accounts
            });
        }


        // GET ACCOUNT BY ID
        [HttpGet("{id}")]
        public ActionResult GetAccountById(int id)
        {
            var account = AccountData.Accounts
                .FirstOrDefault(acc => acc.Id == id);

            if (account == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Account not found"
                });
            }

            return Ok(new
            {
                success = true,
                data = account
            });
        }


        // CREATE ACCOUNT
        [HttpPost]
        public ActionResult CreateAccount(BankAccount account)
        {
            if (
                string.IsNullOrWhiteSpace(account.Name) ||
                string.IsNullOrWhiteSpace(account.Type)
            )
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid account data"
                });
            }

            account.Id = GenerateId.GetNextId(AccountData.Accounts);

            AccountData.Accounts.Add(account);

            return Created("", new
            {
                success = true,
                message = "Account created successfully",
                data = account
            });
        }


        // UPDATE ACCOUNT
        [HttpPut("{id}")]
        public ActionResult UpdateAccount(int id, BankAccount updatedAccount)
        {
            var account = AccountData.Accounts
                .FirstOrDefault(acc => acc.Id == id);

            if (account == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Account not found"
                });
            }

            account.Name = updatedAccount.Name;

            account.Balance = updatedAccount.Balance;

            account.Type = updatedAccount.Type;

            return Ok(new
            {
                success = true,
                message = "Account updated successfully",
                data = account
            });
        }


        // DELETE ACCOUNT
        [HttpDelete("{id}")]
        public ActionResult DeleteAccount(int id)
        {
            var account = AccountData.Accounts
                .FirstOrDefault(acc => acc.Id == id);

            if (account == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Account not found"
                });
            }

            AccountData.Accounts.Remove(account);

            return Ok(new
            {
                success = true,
                message = "Account deleted successfully",
                data = account
            });
        }


        // DEPOSIT MONEY
        [HttpPut("{id}/deposit")]
        public ActionResult DepositMoney(int id, [FromBody] AmountRequest request)
        {
            var account = AccountData.Accounts
                .FirstOrDefault(acc => acc.Id == id);

            if (account == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Account not found"
                });
            }

            if (request.Amount <= 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid amount"
                });
            }

            account.Balance += request.Amount;

            return Ok(new
            {
                success = true,
                message = "Money deposited successfully",
                data = account
            });
        }


        // WITHDRAW MONEY
        [HttpPut("{id}/withdraw")]
        public ActionResult WithdrawMoney(int id, [FromBody] AmountRequest request)
        {
            var account = AccountData.Accounts
                .FirstOrDefault(acc => acc.Id == id);

            if (account == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Account not found"
                });
            }

            if (request.Amount <= 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid amount"
                });
            }

            if (request.Amount > account.Balance)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Insufficient balance"
                });
            }

            account.Balance -= request.Amount;

            return Ok(new
            {
                success = true,
                message = "Money withdrawn successfully",
                data = account
            });
        }


        // TRANSFER MONEY
        [HttpPut("{id}/transfer")]
        public ActionResult TransferMoney(
            int id,
            [FromBody] TransferRequest request
        )
        {
            var sender = AccountData.Accounts
                .FirstOrDefault(acc => acc.Id == id);

            var receiver = AccountData.Accounts
                .FirstOrDefault(acc => acc.Id == request.ReceiverId);

            if (sender == null || receiver == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Sender or receiver not found"
                });
            }

            if (request.Amount <= 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid amount"
                });
            }

            if (sender.Balance < request.Amount)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Insufficient balance"
                });
            }

            sender.Balance -= request.Amount;

            receiver.Balance += request.Amount;

            return Ok(new
            {
                success = true,
                message = "Transfer successful",
                sender,
                receiver
            });
        }
    }


    // REQUEST MODEL FOR DEPOSIT/WITHDRAW
    public class AmountRequest
    {
        public double Amount { get; set; }
    }


    // REQUEST MODEL FOR TRANSFER
    public class TransferRequest
    {
        public int ReceiverId { get; set; }

        public double Amount { get; set; }
    }
}