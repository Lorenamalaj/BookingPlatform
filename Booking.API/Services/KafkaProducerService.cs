using Confluent.Kafka;
using System.Text.Json;

namespace Booking.API.Services;

public class KafkaProducerService
{
    private readonly IProducer<Null, string> _producer;
    private readonly ILogger<KafkaProducerService> _logger;

    public KafkaProducerService(ILogger<KafkaProducerService> logger)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = "localhost:9092"
        };

        _producer = new ProducerBuilder<Null, string>(config).Build();
        _logger = logger;
    }

    public async Task SendEventAsync(string eventType, Guid aggregateId, object payload, string source = "BookingPlatform")
    {
        var eventMessage = new
        {
            EventType = eventType,
            AggregateId = aggregateId,
            Payload = JsonSerializer.Serialize(payload),
            Source = source,
            Version = 1
        };

        var message = new Message<Null, string>
        {
            Value = JsonSerializer.Serialize(eventMessage)
        };

        try
        {
            var result = await _producer.ProduceAsync("booking-events", message);
            _logger.LogInformation("Event sent to Kafka: {EventType} - Offset: {Offset}", eventType, result.Offset);
        }
        catch (ProduceException<Null, string> ex)
        {
            _logger.LogError(ex, "Error sending event to Kafka: {EventType}", eventType);
        }
    }
}