namespace PaymentService.Infrastructure.Kafka;

using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class KafkaTopicInitializer : IHostedService
{
    private readonly string _bootstrapServers;
    private readonly ILogger<KafkaTopicInitializer> _logger;

    public KafkaTopicInitializer(string bootstrapServers, ILogger<KafkaTopicInitializer> logger)
    {
        _bootstrapServers = bootstrapServers;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var adminClient = new AdminClientBuilder(
                new AdminClientConfig { BootstrapServers = _bootstrapServers })
                .Build();

            var topics = new List<TopicSpecification>
            {
                new TopicSpecification 
                { 
                    Name = "risk-evaluation-request", 
                    NumPartitions = 1, 
                    ReplicationFactor = 1 
                },
                new TopicSpecification 
                { 
                    Name = "risk-evaluation-response", 
                    NumPartitions = 1, 
                    ReplicationFactor = 1 
                }
            };

            try
            {
                await adminClient.CreateTopicsAsync(topics, new CreateTopicsOptions 
                { 
                    ValidateOnly = false,
                    OperationTimeout = TimeSpan.FromSeconds(30)
                });
                _logger.LogInformation("✓ Topics creados exitosamente");
            }
            catch (CreateTopicsException ex)
            {
                foreach (var report in ex.Results)
                {
                    if (report.Error.Code == Confluent.Kafka.ErrorCode.TopicAlreadyExists)
                    {
                        _logger.LogInformation("✓ Topic '{Topic}' ya existe", report.Topic);
                    }
                    else
                    {
                        _logger.LogError("✗ Error creando topic '{Topic}': {Error}", report.Topic, report.Error.Reason);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inicializando topics en Kafka");
        }

        await Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
