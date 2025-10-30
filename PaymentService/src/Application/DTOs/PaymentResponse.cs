namespace PaymentService.Application.DTOs;

public class PaymentResponse
{
    public Guid ExternalOperationId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
