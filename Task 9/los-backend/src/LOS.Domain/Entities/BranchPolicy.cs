namespace LOS.Domain.Entities;

public class BranchPolicy
{
    public Guid Id { get; set; }
    public Guid BranchId { get; set; }
    public Branch Branch { get; set; } = null!;
    public decimal MinInterestRate { get; set; }
    public decimal MaxInterestRate { get; set; }
    public decimal HighValueThreshold { get; set; }
}
