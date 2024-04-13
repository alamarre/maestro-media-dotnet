using System.Collections.Immutable;
using System.Text.Json;
using Maestro.Core;
using Maestro.Models;

namespace Maestro.Events;

public static partial class AutoEventMapping
{
    public static ImmutableDictionary<Guid, global::System.Type> IdsToEvents { get; private set; }

    static AutoEventMapping()
    {
        IdsToEvents = ImmutableDictionary<Guid, global::System.Type>.Empty;
        Initialize();
    }

    public static Type? GetEventType(Guid id)
    {
        if (IdsToEvents.TryGetValue(id, out var type))
        {
            return type;
        }

        return null;
    }

    public static EventMessage GetEventMessage<T>(T @event) where T : notnull => GetEventMessage(typeof(T), @event);

    public static Guid GetEventTypeId(Type t)
    {
        var eventAttribute = Attribute.GetCustomAttribute(t, typeof(EventAttribute));
        if (eventAttribute == null)
        {
            throw new InvalidOperationException($"Event t does not have an EventAttribute");
        }

        var eventTypeIdString = ((EventAttribute)eventAttribute).EventId;
        if (!Guid.TryParse(eventTypeIdString, out var eventTypeId))
        {
            throw new InvalidOperationException($"Event t has an invalid EventAttribute");
        }

        return eventTypeId;
    }

    public static EventMessage GetEventMessage(Type t, object @event)
    {
        var eventTypeId = GetEventTypeId(t);
        return new EventMessage(Guid.NewGuid(), eventTypeId, JsonSerializer.Serialize(@event));
    }

    public static ReceivedEvent? ConvertToReceivedMessage(EventMessage eventMessage)
    {
        var eventType = AutoEventMapping.GetEventType(eventMessage.EventTypeId);

        if (eventType == null)
        {
            throw new InvalidOperationException($"Event {eventMessage.EventTypeId} is not registered");
        }

        var @event = JsonSerializer.Deserialize(eventMessage.SerializedEventData, eventType);

        if (@event != null)
        {
            return new ReceivedEvent(eventMessage.EventId, eventMessage.EventTypeId, eventType, @event);
        }

        return null;
    }

    static partial void Initialize();
}
