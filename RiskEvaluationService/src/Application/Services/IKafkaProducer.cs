namespace RiskEvaluationService.Application.Services;

using RiskEvaluationService.Application.DTOs;

public interface IKafkaProducer
{
    Task SendRiskEvaluationResponseAsync(RiskEvaluationResponse response);
}
