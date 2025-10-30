namespace RiskEvaluationService.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using RiskEvaluationService.Domain.Entities;
using RiskEvaluationService.Domain.Repositories;
using RiskEvaluationService.Infrastructure.Data;

public class PaymentRepository : IPaymentRepository
{
    private readonly PaymentDbContext _context;

    public PaymentRepository(PaymentDbContext context) => _context = context;

    public async Task<List<Payment>> GetCustomerTransactionsForDateAsync(Guid customerId, DateTime date)
    {
        return await _context.Payments
            .Where(p => p.CustomerId == customerId && p.CreatedAt.Date == date && p.Status == "accepted")
            .ToListAsync();
    }
}
