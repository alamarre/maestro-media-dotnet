using Maestro.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Maestro.Events.Handlers;

public interface IEventProcessor {
    Task ProcessEvent<T>(T @event, CancellationToken cancellationToken = default) where T : notnull;
}

public sealed class EventProcessor(IServiceProvider serviceProvider) : IEventProcessor {
    public async Task ProcessEvent<T>(T @event, CancellationToken cancellationToken = default) where T : notnull {
        var handlers = serviceProvider.GetServices<IEventHandler<T>>();
        var tasks = handlers.Select(handler => handler.Handle(@event, cancellationToken));
        await Task.WhenAll(tasks);
    }
}