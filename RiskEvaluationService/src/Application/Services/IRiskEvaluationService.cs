namespace RiskEvaluationService.Application.Services;

using RiskEvaluationService.Application.DTOs;

public interface IRiskEvaluationService
{
    Task<RiskEvaluationResponse> EvaluateRiskAsync(RiskEvaluationRequest request);
}
