using System.Text.Json;
using Amazon.SQS;
using Maestro.Models;
using Maestro.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Maestro.Events;

public sealed class SqsEventReceiver(
    IOptions<EventOptions> options,
    ILogger<SqsEventReceiver> logger,
    AmazonSQSClient sqsClient
) : IEventReceiver
{
    private readonly EventOptions eventOptions = options.Value;

    async Task IEventReceiver.DeleteEvent(ReceivedEvent @event, CancellationToken cancellationToken)
    {
        await sqsClient.DeleteMessageAsync(
            new Amazon.SQS.Model.DeleteMessageRequest
            {
                QueueUrl = eventOptions.SqsQueueUrl, ReceiptHandle = @event.ReceiptHandle
            }, cancellationToken);
    }

    async Task<ReceivedEvent?> IEventReceiver.Receive(CancellationToken cancellationToken)
    {
        var result = await sqsClient.ReceiveMessageAsync(
            new Amazon.SQS.Model.ReceiveMessageRequest
            {
                QueueUrl = eventOptions.SqsQueueUrl, MaxNumberOfMessages = 1,
            }, cancellationToken);

        var message = result.Messages.FirstOrDefault();
        if (message == null)
        {
            return null;
        }

        logger.LogInformation("Received message {MessageId}", message.MessageId);
        var body = message.Body;
        var eventMessage = JsonSerializer.Deserialize<EventMessage>(body);
        if (eventMessage == null)
        {
            return null;
        }

        var converted = AutoEventMapping.ConvertToReceivedMessage(eventMessage);
        if (converted == null)
        {
            return null;
        }

        return converted with { ReceiptHandle = message.ReceiptHandle };
    }
}
