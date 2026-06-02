using System.ComponentModel.DataAnnotations;

namespace BankApi.Application.DTOs.Requests;

public class UpdateAccountRequest
{
    [Required, MinLength(3), MaxLength(100)]
    public string HolderName { get; set; } = default!;

    [Required, EmailAddress, MaxLength(150)]
    public string Email { get; set; } = default!;

    [Required]
    public string Type { get; set; } = default!;

    public bool IsActive { get; set; }
}
