using System.ComponentModel.DataAnnotations;

namespace BankApi.Application.DTOs.Requests;

public class TransferRequest
{
    [Required]
    public Guid ReceiverId { get; set; }

    [Range(typeof(decimal), "0.01", "999999999999")]
    public decimal Amount { get; set; }
}
