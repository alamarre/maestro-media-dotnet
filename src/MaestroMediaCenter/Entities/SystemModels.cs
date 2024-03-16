using System.ComponentModel.DataAnnotations;

namespace Maestro.Entities;

public record OutboxEvent
{
    [Key] public Guid OutboxEventId { get; set; }
    public Guid EventType { get; set; }
    public string EventData { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedDate { get; set; }
    public int RetryCount { get; set; } = 0;
    public bool Processed { get; set; } = false;
    public string? ErrorMessage { get; set; }
}
