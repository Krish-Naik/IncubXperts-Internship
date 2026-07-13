namespace BankApi.Domain.Constants;

public static class AppPermissions
{
    public const string AccountsOwnRead = "accounts.own.read";
    public const string AccountsOwnDeposit = "accounts.own.deposit";
    public const string AccountsOwnWithdraw = "accounts.own.withdraw";
    public const string TransactionsOwnRead = "transactions.own.read";

    public const string AccountsAllRead = "accounts.all.read";
    public const string AccountsCashDeposit = "accounts.cash.deposit";
    public const string AccountsCashWithdraw = "accounts.cash.withdraw";
    public const string TransactionsTransfer = "transactions.transfer";

    public const string AccountsCreate = "accounts.create";
    public const string AccountsUpdate = "accounts.update";
    public const string AccountsClose = "accounts.close";
    public const string TransactionsAllRead = "transactions.all.read";

    public const string AdminUsersManage = "admin.users.manage";

    public static IReadOnlyList<string> GetByRole(string role) =>
        role switch
        {
            AppRoles.Admin => new[]
            {
                AccountsOwnRead,
                AccountsOwnDeposit,
                AccountsOwnWithdraw,
                TransactionsOwnRead,
                AccountsAllRead,
                AccountsCashDeposit,
                AccountsCashWithdraw,
                TransactionsTransfer,
                AccountsCreate,
                AccountsUpdate,
                AccountsClose,
                TransactionsAllRead,
                AdminUsersManage,
            },
            AppRoles.Manager => new[]
            {
                AccountsAllRead,
                AccountsCashDeposit,
                AccountsCashWithdraw,
                TransactionsTransfer,
                AccountsCreate,
                AccountsUpdate,
                AccountsClose,
                TransactionsAllRead,
            },
            AppRoles.Teller => new[]
            {
                AccountsAllRead,
                AccountsCashDeposit,
                AccountsCashWithdraw,
                TransactionsTransfer,
            },
            _ => new[]
            {
                AccountsOwnRead,
                AccountsOwnDeposit,
                AccountsOwnWithdraw,
                TransactionsOwnRead,
            },
        };
}
