namespace Maestro.Entities;

public interface IOutboxEventPublisher {
    Task Publish(List<OutboxEvent> outboxEvents, CancellationToken cancellationToken = default);
}