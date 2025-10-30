namespace PaymentService.Application.Services;
using PaymentService.Application.DTOs;

public interface IPaymentService
{
    Task<PaymentResponse> CreatePaymentAsync(CreatePaymentRequest request);
    Task<PaymentResponse> GetPaymentAsync(Guid externalOperationId);
    Task<bool> UpdatePaymentStatusAsync(Guid externalOperationId, string status);
}
