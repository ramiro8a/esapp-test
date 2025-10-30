namespace PaymentService.Domain.Repositories;
using PaymentService.Domain.Entities;

public interface IPaymentRepository
{
    Task AddAsync(Payment payment);
    Task UpdateAsync(Payment payment);
    Task<Payment?> GetByExternalOperationIdAsync(Guid externalOperationId);
}
