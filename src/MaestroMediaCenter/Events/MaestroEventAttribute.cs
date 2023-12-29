namespace Maestro.Events;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class MaestroEventAttribute : Attribute {
    public string EventId { get; }

    public MaestroEventAttribute(string eventId) {
        EventId = eventId;
    }
}