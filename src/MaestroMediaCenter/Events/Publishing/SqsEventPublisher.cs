using System.Text.Json;
using Amazon.SQS;
using Maestro.Models;
using Maestro.Options;
using Microsoft.Extensions.Options;

namespace Maestro.Events;

public sealed class SqsEventPublisher(
    IOptions<Maestro.Options.EventOptions> options,
    ILogger<SqsEventPublisher> logger,
    AmazonSQSClient sqsClient
    ) : IEventPublisher
{
    private readonly EventOptions eventOptions = options.Value;
    async Task IEventPublisher.Publish<T>(T @event, CancellationToken cancellationToken)
    {
        logger.LogInformation("Publishing event {Event}", @event);
        // retrieve the event ID from the attribute
        var message = AutoEventMapping.GetEventMessage(typeof(T), @event);
        //queue.Enqueue(JsonSerializer.Serialize(message));
        var json = JsonSerializer.Serialize(message);
        var request = new Amazon.SQS.Model.SendMessageRequest {
            QueueUrl = eventOptions.SqsQueueUrl,
            MessageBody = json
        };
        await sqsClient.SendMessageAsync(request, cancellationToken);
    }

    Task IEventPublisher.Publish<T>(List<T> events, CancellationToken cancellationToken)
    {
        logger.LogInformation("Publishing {eventCount} events", events.Count);
        var messages = events.Select(e => AutoEventMapping.GetEventMessage(typeof(T), e));
        var serializedMessages = messages.Select(m => JsonSerializer.Serialize(m));
        var request = new Amazon.SQS.Model.SendMessageBatchRequest {
            QueueUrl = eventOptions.SqsQueueUrl,
            Entries = serializedMessages.Select((m, i) => new Amazon.SQS.Model.SendMessageBatchRequestEntry {
                Id = i.ToString(),
                MessageBody = m
            }).ToList()
        };
        return sqsClient.SendMessageBatchAsync(request, cancellationToken);
    }

    Task IEventPublisher.Publish(List<EventMessage> events, CancellationToken cancellationToken)
    {
        logger.LogInformation("Publishing {eventCount} events", events.Count);
        var serializedMessages = events.Select(m => JsonSerializer.Serialize(m));
        var request = new Amazon.SQS.Model.SendMessageBatchRequest {
            QueueUrl = eventOptions.SqsQueueUrl,
            Entries = serializedMessages.Select((m, i) => new Amazon.SQS.Model.SendMessageBatchRequestEntry {
                Id = i.ToString(),
                MessageBody = m
            }).ToList()
        };
        return sqsClient.SendMessageBatchAsync(request, cancellationToken);
    }
}
