namespace PaymentService.Application.DTOs;

public interface RiskEvaluationRequestMessage
{
    Guid ExternalOperationId { get; }
    Guid CustomerId { get; }
    decimal Amount { get; }
}

public interface RiskEvaluationResponseMessage
{
    Guid ExternalOperationId { get; }
    string Status { get; }
}
