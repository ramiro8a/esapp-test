namespace PaymentService.Application.DTOs;

public class CreatePaymentRequest
{
    public Guid CustomerId { get; set; }
    public Guid ServiceProviderId { get; set; }
    public int PaymentMethodId { get; set; }
    public decimal Amount { get; set; }
}
