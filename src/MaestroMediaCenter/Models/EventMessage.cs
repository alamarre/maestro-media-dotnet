namespace Maestro.Models;

public record EventMessage(Guid EventId, Guid EventTypeId, string SerializedEventData);

public record ReceivedEvent(Guid EventId, Guid EventTypeId, Type type, object EventData, string? ReceiptHandle = null);