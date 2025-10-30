namespace PaymentService.Infrastructure.Kafka;
using Confluent.Kafka;
using PaymentService.Application.DTOs;
using PaymentService.Application.Services;
using System.Text.Json;

public class KafkaProducer : IKafkaProducer
{
    private readonly IProducer<string, string> _producer;
    private const string Topic = "risk-evaluation-request";

    public KafkaProducer(string bootstrapServers)
    {
        var config = new ProducerConfig { BootstrapServers = bootstrapServers };
        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task SendRiskEvaluationRequestAsync(RiskEvaluationRequest request)
    {
        var message = JsonSerializer.Serialize(request);
        await _producer.ProduceAsync(Topic, new Message<string, string>
        {
            Key = request.ExternalOperationId.ToString(),
            Value = message
        });
    }

    public void Dispose() => _producer?.Dispose();
}
