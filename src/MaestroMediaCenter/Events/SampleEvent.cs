using System.Diagnostics.Tracing;

namespace Maestro.Events;

[Maestro.Core.Event("testId")]
public record SampleEvent(string X);