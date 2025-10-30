using Confluent.Kafka;
using System.Text.Json;
using RiskEvaluationService.Application.DTOs;

namespace RiskEvaluationService.Infrastructure.Kafka;

public class RiskResponseProducer
{
    private readonly IProducer<string, string> _producer;

    public RiskResponseProducer(string kafkaServers)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = kafkaServers,
            Acks = Acks.All,
            MessageSendMaxRetries = 3
        };
        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task PublishResponseAsync(RiskEvaluationResponse response)
    {
        try
        {
            var json = JsonSerializer.Serialize(response);
            var message = new Message<string, string>
            {
                Key = response.ExternalOperationId.ToString(),
                Value = json
            };

            await _producer.ProduceAsync("risk-evaluation-response", message);
            Console.WriteLine($"✅ Respuesta publicada: {response.ExternalOperationId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error publicando respuesta: {ex.Message}");
            throw;
        }
    }
}
