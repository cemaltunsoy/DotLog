using Core.Enums;

namespace Core.Interfaces;

public interface ILogSource : IDisposable
{
    string Name { get; }
    LogSourceType Type { get; }
    LogSourceConfig Config { get; }
    Task InitializeAsync(CancellationToken cancellationToken = default);
    IAsyncEnumerable<LogEntry> GetLogsAsync(CancellationToken cancellationToken = default);
}