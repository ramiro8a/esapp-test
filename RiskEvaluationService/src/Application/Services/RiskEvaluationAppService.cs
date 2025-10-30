using RiskEvaluationService.Application.DTOs;
using RiskEvaluationService.Domain.Repositories;

namespace RiskEvaluationService.Application.Services;

public class RiskEvaluationAppService : IRiskEvaluationService
{
    private readonly IPaymentRepository _paymentRepository;
    private const decimal DailyLimit = 5000m;
    private const decimal SingleTransactionLimit = 2000m;

    public RiskEvaluationAppService(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<RiskEvaluationResponse> EvaluateRiskAsync(RiskEvaluationRequest request)
    {
        string status = "accepted";

        // Regla 1: Operaciones > 2000 Bs se rechazan
        if (request.Amount > SingleTransactionLimit)
        {
            status = "denied";
            Console.WriteLine($"⛔ Rechazado: Monto {request.Amount} > {SingleTransactionLimit}");
        }
        else
        {
            // Regla 2: Acumulado diario > 5000 Bs se rechaza
            var today = DateTime.UtcNow.Date;
            
            // Obtener transacciones aceptadas de HOY desde BD
            var todayTransactions = await _paymentRepository.GetCustomerTransactionsForDateAsync(request.CustomerId, today);
            var dailyTotal = todayTransactions.Sum(t => t.Amount) + request.Amount;

            if (dailyTotal > DailyLimit)
            {
                status = "denied";
                Console.WriteLine($"⛔ Rechazado: Acumulado diario {dailyTotal} > {DailyLimit}");
            }
            else
            {
                Console.WriteLine($"✅ Aceptada: {request.ExternalOperationId}");
            }
        }

        return new RiskEvaluationResponse
        {
            ExternalOperationId = request.ExternalOperationId,
            Status = status
        };
    }
}
