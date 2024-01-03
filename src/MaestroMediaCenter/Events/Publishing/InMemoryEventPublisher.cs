using System.Collections.Concurrent;
using System.Text.Json;
using Maestro.Core;
using Maestro.Models;

namespace Maestro.Events;

public class InMemoryEventPublisher(ILogger<InMemoryEventPublisher> logger) : IEventPublisher, IEventReceiver {
    
    ConcurrentQueue<string> queue = new ConcurrentQueue<string>();

    Task IEventReceiver.DeleteEvent(ReceivedEvent @event, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    Task IEventPublisher.Publish<T>(T @event, CancellationToken cancellationToken) {
        logger.LogInformation("Publishing event {Event}", @event);
        // retrieve the event ID from the attribute
        var message = AutoEventMapping.GetEventMessage(typeof(T), @event);
        queue.Enqueue(JsonSerializer.Serialize(message));
        return Task.CompletedTask;
    }


    async Task IEventPublisher.Publish<T>(List<T> events, CancellationToken cancellationToken)
    {
        foreach(var @event in events) {
            await ((IEventPublisher)this).Publish(@event, cancellationToken);
        }
    }

    Task IEventPublisher.Publish(List<EventMessage> events, CancellationToken cancellationToken)
    {
        foreach(var @event in events) {
            queue.Enqueue(JsonSerializer.Serialize(@event));
        }
        return Task.CompletedTask;
    }

    async Task<ReceivedEvent?> IEventReceiver.Receive(CancellationToken cancellationToken)
    {
        ReceivedEvent? result = null;
        if(!queue.TryDequeue(out var message)) {
            await Task.Delay(100, cancellationToken);
            return result;
        }

        if(message == null) {
            return result;
        }

        var eventMessage = JsonSerializer.Deserialize<EventMessage>(message);
        if(eventMessage == null) {
            return result;
        }

        result = AutoEventMapping.ConvertToReceivedMessage(eventMessage);
        
        return result;
    }
}
