namespace Maestro.Models;

public record ShowProgress(string show, string? season, string episode, string status, int progress, long expires);
