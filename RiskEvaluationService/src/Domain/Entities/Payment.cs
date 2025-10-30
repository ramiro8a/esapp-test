namespace RiskEvaluationService.Domain.Entities;

public class Payment
{
    public Guid Id { get; set; }
    public Guid ExternalOperationId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid ServiceProviderId { get; set; }
    public int PaymentMethodId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = PaymentStatus.Evaluating;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public static class PaymentStatus
{
    public const string Evaluating = "evaluating";
    public const string Accepted = "accepted";
    public const string Denied = "denied";
}
