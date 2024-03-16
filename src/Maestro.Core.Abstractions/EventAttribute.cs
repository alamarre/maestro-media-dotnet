namespace Maestro.Core;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class EventAttribute(string eventId) : Attribute
{
    public string EventId => eventId;
}
