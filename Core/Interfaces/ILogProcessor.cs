namespace Core.Interfaces;

public interface ILogProcessor
{
    Task ProcessLogEntryAsync(LogEntry entry, CancellationToken cancellationToken = default);
    Task ProcessBatchAsync(IEnumerable<LogEntry> entries, CancellationToken cancellationToken = default);
}