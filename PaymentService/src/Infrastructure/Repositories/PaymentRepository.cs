namespace PaymentService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Repositories;
using PaymentService.Infrastructure.Data;

public class PaymentRepository : IPaymentRepository
{
    private readonly PaymentDbContext _context;
    public PaymentRepository(PaymentDbContext context) => _context = context;

    public async Task AddAsync(Payment payment)
    {
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Payment payment)
    {
        _context.Payments.Update(payment);
        await _context.SaveChangesAsync();
    }

    public async Task<Payment?> GetByExternalOperationIdAsync(Guid externalOperationId)
    {
        return await _context.Payments.FirstOrDefaultAsync(p => p.ExternalOperationId == externalOperationId);
    }
}
