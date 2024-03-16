using Maestro.Events;
using Maestro.Models;

namespace Maestro.Entities;

public class OutboxEventPublisher(IEventPublisher eventPublisher) : IOutboxEventPublisher
{
    async Task IOutboxEventPublisher.Publish(List<OutboxEvent> outboxEvents, CancellationToken cancellationToken)
    {
        var messages = outboxEvents.Select(x => new EventMessage(x.OutboxEventId, x.EventType, x.EventData)).ToList();
        await eventPublisher.Publish(messages, cancellationToken);
    }
}
