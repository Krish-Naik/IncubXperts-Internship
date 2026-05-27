namespace BankApi.Models
{
    public class BankAccount
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public double Balance { get; set; }

        public string Type { get; set; } = string.Empty;
    }
}