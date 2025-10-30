namespace PaymentService.Infrastructure.Services;

using Microsoft.Extensions.Logging;
using PaymentService.Application.DTOs;
using PaymentService.Application.Services;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Repositories;

public class PaymentAppService : IPaymentService
{
    private readonly IPaymentRepository _repository;
    private readonly PaymentEventPublisher _eventPublisher;
    private readonly ILogger<PaymentAppService> _logger;

    public PaymentAppService(
        IPaymentRepository repository,
        PaymentEventPublisher eventPublisher,
        ILogger<PaymentAppService> logger)
    {
        _repository = repository;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<PaymentResponse> CreatePaymentAsync(CreatePaymentRequest request)
    {
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            ExternalOperationId = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            ServiceProviderId = request.ServiceProviderId,
            PaymentMethodId = request.PaymentMethodId,
            Amount = request.Amount,
            Status = PaymentStatus.Evaluating,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(payment);
        await _eventPublisher.PublishRiskEvaluationRequestAsync(
            payment.ExternalOperationId,
            payment.CustomerId,
            payment.Amount);

        _logger.LogInformation("Payment created: {Id}", payment.ExternalOperationId);
        return new PaymentResponse
        {
            ExternalOperationId = payment.ExternalOperationId,
            CreatedAt = payment.CreatedAt,
            Status = payment.Status,
            Amount = payment.Amount
        };
    }

    public async Task<PaymentResponse> GetPaymentAsync(Guid externalOperationId)
    {
        var payment = await _repository.GetByExternalOperationIdAsync(externalOperationId);
        if (payment == null)
            throw new InvalidOperationException($"Payment not found: {externalOperationId}");

        return new PaymentResponse
        {
            ExternalOperationId = payment.ExternalOperationId,
            CreatedAt = payment.CreatedAt,
            Status = payment.Status,
            Amount = payment.Amount
        };
    }

    public async Task<bool> UpdatePaymentStatusAsync(Guid externalOperationId, string status)
    {
        var payment = await _repository.GetByExternalOperationIdAsync(externalOperationId);
        if (payment == null) return false;

        payment.Status = status;
        payment.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(payment);
        return true;
    }
}
