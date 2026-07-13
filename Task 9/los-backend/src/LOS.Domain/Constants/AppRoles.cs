namespace LOS.Domain.Constants;

public static class AppRoles
{
    public const string SystemAdministrator = "SystemAdministrator";
    public const string VerificationOfficer = "VerificationOfficer";
    public const string BranchManager = "BranchManager";
    public const string Customer = "Customer";
    public const string BrokerAgent = "BrokerAgent";

    public static readonly string[] All =
    [
        SystemAdministrator,
        VerificationOfficer,
        BranchManager,
        Customer,
        BrokerAgent,
    ];
}
