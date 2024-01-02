namespace Maestro.Core;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
#pragma warning disable CS9113 // Parameter is unread.
public class EventAttribute(string eventId) : Attribute {
#pragma warning restore CS9113 // Parameter is unread.

}