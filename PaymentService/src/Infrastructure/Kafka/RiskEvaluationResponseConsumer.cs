using MassTransit;
using PaymentService.Application.DTOs;
using PaymentService.Domain.Repositories;

namespace PaymentService.Infrastructure.Kafka;

public class RiskEvaluationResponseConsumer : IConsumer<RiskEvaluationResponseMessage>
{
    private readonly IPaymentRepository _repository;

    public RiskEvaluationResponseConsumer(IPaymentRepository repository)
    {
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<RiskEvaluationResponseMessage> context)
    {
        var message = context.Message;
        var payment = await _repository.GetByExternalOperationIdAsync(message.ExternalOperationId);
        
        if (payment != null)
        {
            payment.Status = message.Status;
            payment.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(payment);
        }
    }
}
