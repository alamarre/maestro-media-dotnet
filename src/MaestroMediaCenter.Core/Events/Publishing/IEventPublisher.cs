using Maestro.Models;

namespace Maestro.Events;

public interface IEventPublisher
{
    Task Publish<T>(T @event, CancellationToken cancellationToken = default) where T : notnull;

    Task Publish<T>(List<T> @events, CancellationToken cancellationToken = default) where T : notnull;

    Task Publish(List<EventMessage> @events, CancellationToken cancellationToken = default);
}
