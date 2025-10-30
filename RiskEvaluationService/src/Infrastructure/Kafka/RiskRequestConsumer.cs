using Confluent.Kafka;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using RiskEvaluationService.Application.DTOs;
using RiskEvaluationService.Application.Services;
using RiskEvaluationService.Infrastructure.Data;

namespace RiskEvaluationService.Infrastructure.Kafka;

public class RiskRequestConsumer : BackgroundService
{
    private readonly IConsumer<string, string> _consumer;
    private readonly RiskResponseProducer _producer;
    private readonly IServiceProvider _serviceProvider;

    public RiskRequestConsumer(string kafkaServers, RiskResponseProducer producer, IServiceProvider serviceProvider)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = kafkaServers,
            GroupId = "risk-evaluation-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true
        };

        _consumer = new ConsumerBuilder<string, string>(config).Build();
        _producer = producer;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe("risk-evaluation-request");
        Console.WriteLine("🔄 Escuchando risk-evaluation-request...");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var message = _consumer.Consume(TimeSpan.FromSeconds(5));
                if (message == null) continue;

                try
                {
                    var request = JsonSerializer.Deserialize<RiskEvaluationRequest>(message.Message.Value);
                    if (request == null) continue;

                    Console.WriteLine($"📨 Evaluando: {request.ExternalOperationId}");

                    // Obtener servicios del scope
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var riskService = scope.ServiceProvider.GetRequiredService<IRiskEvaluationService>();
                        var dbContext = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();

                        var result = await riskService.EvaluateRiskAsync(request);

                        // Actualizar estado del payment en BD
                        var payment = await dbContext.Payments.FirstOrDefaultAsync(p => p.ExternalOperationId == request.ExternalOperationId);

                        if (payment != null)
                        {
                            payment.Status = result.Status;
                            payment.UpdatedAt = DateTime.UtcNow;
                            await dbContext.SaveChangesAsync();
                            Console.WriteLine($"💾 Payment actualizado: {payment.ExternalOperationId} → {result.Status}");
                        }
                        else
                        {
                            Console.WriteLine($"⚠️ Payment no encontrado: {request.ExternalOperationId}");
                        }

                        await _producer.PublishResponseAsync(result);

                        Console.WriteLine($"✅ Resultado publicado: {request.ExternalOperationId} - {result.Status}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error procesando mensaje: {ex.Message}");
                }
            }
        }
        finally
        {
            _consumer.Close();
        }
    }
}
