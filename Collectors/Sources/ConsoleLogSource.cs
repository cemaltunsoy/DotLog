using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Core;
using Core.Enums;
using Core.Interfaces;

namespace Collectors.Sources;

public class ConsoleLogSource : ILogSource
{
    private readonly LogSourceConfig _config;
    private readonly Random _random;
    private bool _disposed;
    private readonly Channel<LogEntry> _channel;


    public ConsoleLogSource(LogSourceConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _random = new Random();
        _channel = Channel.CreateUnbounded<LogEntry>();
    }

    public string Name => _config.Name;
    public LogSourceType Type => LogSourceType.Console;
    public LogSourceConfig Config => _config;

    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Initializing {Name}...");
        return Task.CompletedTask;
    }

    public async Task LogAsync(LogEntry entry, CancellationToken cancellationToken = default)
    {
        await _channel.Writer.WriteAsync(entry, cancellationToken);
    }

    public async IAsyncEnumerable<LogEntry> GetLogsAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            while (await _channel.Reader.WaitToReadAsync(cancellationToken))
            {
                if (_channel.Reader.TryRead(out var entry))
                {
                    yield return entry;
                }
            }
        }
    }
    
    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
        }
    }
}