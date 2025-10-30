using Confluent.Kafka;
using System.Text.Json;

namespace PaymentService.Infrastructure.Services;

public class PaymentEventPublisher
{
    private readonly IProducer<string, string> _kafkaProducer;

    public PaymentEventPublisher(string kafkaServers)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = kafkaServers,
            Acks = Acks.All,
            MessageSendMaxRetries = 3
        };
        _kafkaProducer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task PublishRiskEvaluationRequestAsync(Guid externalOperationId, Guid customerId, decimal amount)
    {
        try
        {
            var message = new
            {
                ExternalOperationId = externalOperationId,
                CustomerId = customerId,
                Amount = amount
            };

            var json = JsonSerializer.Serialize(message);
            var kafkaMessage = new Message<string, string>
            {
                Key = externalOperationId.ToString(),
                Value = json
            };

            await _kafkaProducer.ProduceAsync("risk-evaluation-request", kafkaMessage);
            Console.WriteLine($"✅ Risk evaluation request published: {externalOperationId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error publishing: {ex.Message}");
            throw;
        }
    }
}
