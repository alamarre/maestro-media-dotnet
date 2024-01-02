using System.Text.Json;
using Maestro.Core;
using Maestro.Events;
using Microsoft.EntityFrameworkCore;

namespace Maestro.Entities;

public class TransactionalOutboxEventProducer : ITransactionalOutboxEventProducer {

    
    public async Task Produce<T>(T @event, MediaDbContext db, CancellationToken cancellationToken) where T : notnull {

        var eventId = AutoEventMapping.GetEventTypeId(typeof(T));
        var outboxEvent = new OutboxEvent {
            OutboxEventId = Guid.NewGuid(),
            CreatedDate = DateTime.UtcNow,
            EventType = eventId,
            EventData = JsonSerializer.Serialize(@event)
        };

        await db.OutboxEvent.AddAsync(outboxEvent);
    }
}