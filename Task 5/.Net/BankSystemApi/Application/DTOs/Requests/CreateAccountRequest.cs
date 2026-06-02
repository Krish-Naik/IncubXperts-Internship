using System.ComponentModel.DataAnnotations;

namespace BankApi.Application.DTOs.Requests;

public class CreateAccountRequest
{
    [Required, MinLength(3), MaxLength(100)]
    public string HolderName { get; set; } = default!;

    [Required, EmailAddress, MaxLength(150)]
    public string Email { get; set; } = default!;

    [Required]
    public string Type { get; set; } = default!;

    [Range(typeof(decimal), "0", "999999999999")]
    public decimal OpeningBalance { get; set; }
}