using BankApi.Models;

namespace BankApi.Data
{
    public static class AccountData
    {
        public static List<BankAccount> Accounts = new List<BankAccount>
        {
            new BankAccount
            {
                Id = 1,
                Name = "Krish",
                Balance = 5000,
                Type = "Savings"
            }
        };
    }
}