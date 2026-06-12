namespace BankApi.Authorization;

public static class AppPolicies
{
    public const string AccountsOwnRead = "AccountsOwnRead";
    public const string AccountsOwnDeposit = "AccountsOwnDeposit";
    public const string AccountsOwnWithdraw = "AccountsOwnWithdraw";
    public const string TransactionsOwnRead = "TransactionsOwnRead";

    public const string AccountsAllRead = "AccountsAllRead";
    public const string AccountsCashDeposit = "AccountsCashDeposit";
    public const string AccountsCashWithdraw = "AccountsCashWithdraw";
    public const string TransactionsTransfer = "TransactionsTransfer";

    public const string AccountsCreate = "AccountsCreate";
    public const string AccountsUpdate = "AccountsUpdate";
    public const string AccountsClose = "AccountsClose";
    public const string TransactionsAllRead = "TransactionsAllRead";

    public const string AdminUsersManage = "AdminUsersManage";
}
