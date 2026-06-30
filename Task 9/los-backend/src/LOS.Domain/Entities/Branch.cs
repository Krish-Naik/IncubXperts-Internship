namespace LOS.Domain.Entities;

public class Branch
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public ICollection<InternalUser> Users { get; set; } = [];
}
