using Maestro.Models;

namespace Maestro.Events;

public interface IEventReceiver {
    Task<ReceivedEvent?> Receive(CancellationToken cancellationToken);
}