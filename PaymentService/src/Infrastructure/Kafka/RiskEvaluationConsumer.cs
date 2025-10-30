namespace PaymentService.Infrastructure.Kafka;

using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PaymentService.Application.DTOs;
using PaymentService.Application.Services;
using System.Text.Json;

public class RiskEvaluationConsumer : BackgroundService
{
    private readonly IConsumer<string, string> _consumer;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RiskEvaluationConsumer> _logger;
    private const string Topic = "risk-evaluation-response";

    public RiskEvaluationConsumer(
        string bootstrapServers,
        IServiceProvider serviceProvider,
        ILogger<RiskEvaluationConsumer> logger)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = "payment-service-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true
        };
        _consumer = new ConsumerBuilder<string, string>(config).Build();
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe(Topic);
        _logger.LogInformation("✓ RiskEvaluationConsumer iniciado");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var message = _consumer.Consume(stoppingToken);
                if (message != null)
                {
                    var response = JsonSerializer.Deserialize<RiskEvaluationResponse>(message.Message.Value);
                    if (response != null)
                    {
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();
                            await paymentService.UpdatePaymentStatusAsync(response.ExternalOperationId, response.Status);
                            _logger.LogInformation("Pago actualizado: {Id} -> {Status}", response.ExternalOperationId, response.Status);
                        }
                    }
                }
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex) { _logger.LogError(ex, "Error en consumer"); }
        }
        _consumer.Close();
    }
}
