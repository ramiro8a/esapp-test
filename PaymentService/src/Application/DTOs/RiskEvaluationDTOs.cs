namespace PaymentService.Application.DTOs;

public class RiskEvaluationRequest
{
    public Guid ExternalOperationId { get; set; }
    public Guid CustomerId { get; set; }
    public decimal Amount { get; set; }
}

public class RiskEvaluationResponse
{
    public Guid ExternalOperationId { get; set; }
    public string Status { get; set; } = string.Empty;
}
