using BankApi.Models;

namespace BankApi.Utils
{
    public static class GenerateId
    {
        public static int GetNextId(List<BankAccount> accounts)
        {
            if (accounts.Count == 0){
                return 1;
            }

            return accounts.Last().Id + 1;
        }
    }
}