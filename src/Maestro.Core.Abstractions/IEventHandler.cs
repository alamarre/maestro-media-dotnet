namespace Maestro.Core;

public interface IEventHandler<T> where T : notnull {
    Task Handle(T @event, CancellationToken cancellationToken = default);
}