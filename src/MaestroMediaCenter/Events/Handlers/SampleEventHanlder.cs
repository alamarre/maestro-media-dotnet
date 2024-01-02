using Maestro.Core;
using Maestro.Entities;

namespace Maestro.Events;

public class SampleEventHandler : IEventHandler<SampleEvent> {
    private readonly ILogger<SampleEventHandler> _logger;

    public SampleEventHandler(ILogger<SampleEventHandler> logger) {
        _logger = logger;
    }

    public async Task Handle(SampleEvent @event, CancellationToken cancellationToken) {
       await Task.CompletedTask;
    }
}
