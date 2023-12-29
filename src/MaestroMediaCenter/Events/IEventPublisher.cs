namespace Maestro.Events;

public interface IEventPublisher {
    Task Publish<T>(T @event) where T : notnull;
}
