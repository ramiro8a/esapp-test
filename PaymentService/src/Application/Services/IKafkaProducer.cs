namespace PaymentService.Application.Services;
using PaymentService.Application.DTOs;

public interface IKafkaProducer
{
    Task SendRiskEvaluationRequestAsync(RiskEvaluationRequest request);
}
