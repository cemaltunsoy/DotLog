using Core.Enums;

namespace Core;

public record LogEntry
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; init; }
    public string Level { get; init; }
    public string Message { get; init; }
    public string Source { get; init; }
    public Dictionary<string, object> Properties { get; init; } = new();
    public string? Exception { get; init; }
    public string? StackTrace { get; init; }
    public string? ApplicationName { get; init; }
    public string? Environment { get; init; }
    public string? ClassName { get; init; }
    public TimeSpan? ExecutionTime { get; init; }
}