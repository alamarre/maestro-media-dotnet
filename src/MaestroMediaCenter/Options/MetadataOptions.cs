namespace Maestro.Options;

public sealed class MetadataOptions {
    public const string SectionName = "Metadata";
    public string? TmdbKey { get; set; }
}

public sealed class EventOptions {
    public const string SectionName = "Events";
    public string? SqsQueueUrl { get; set; }
}