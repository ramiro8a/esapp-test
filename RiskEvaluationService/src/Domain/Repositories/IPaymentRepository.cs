namespace RiskEvaluationService.Domain.Repositories;

using RiskEvaluationService.Domain.Entities;

public interface IPaymentRepository
{
    Task<List<Payment>> GetCustomerTransactionsForDateAsync(Guid customerId, DateTime date);
}
