namespace LOS.Domain.Entities;

public class CoApplicant
{
    public Guid Id { get; set; }
    public Guid LoanApplicationId { get; set; }
    public LoanApplication LoanApplication { get; set; } = null!;
    public string FullName { get; set; } = string.Empty;
    public string Pan { get; set; } = string.Empty;
    public decimal MonthlyIncome { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
