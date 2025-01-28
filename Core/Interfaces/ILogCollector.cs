namespace Core.Interfaces;

public interface ILogCollector : IDisposable
{
    Task StartCollectingAsync(ILogSource source, CancellationToken cancellationToken = default);
    Task StopCollectingAsync(string sourceName);
    Task<IReadOnlyCollection<string>> GetActiveSourcesAsync();
    Task SendLogAsync(LogEntry entry, CancellationToken cancellationToken = default);
}