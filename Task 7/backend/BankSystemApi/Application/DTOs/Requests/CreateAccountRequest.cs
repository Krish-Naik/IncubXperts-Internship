using System.ComponentModel.DataAnnotations;

namespace BankApi.Application.DTOs.Requests;

public class CreateAccountRequest
{
    [Required]
    public Guid AppUserId { get; set; }

    [Required]
    public string Type { get; set; } = default!;

    [Range(typeof(decimal), "0", "999999999999")]
    public decimal OpeningBalance { get; set; }
}
