using System.Threading.Channels;
using Core;
using Core.Interfaces;

namespace Collectors;

/// <summary>
/// Collects logs from multiple sources and processes them in batches.
/// </summary>
public class BufferedLogCollector : ILogCollector
{
    private readonly Channel<LogEntry> _channel;
    private readonly ILogProcessor _processor;
    private readonly Dictionary<string, ILogSource> _activeSources;
    private readonly SemaphoreSlim _sourcesSemaphore;
    private readonly int _batchSize;
    private bool _disposed;

    public BufferedLogCollector(ILogProcessor processor, int batchSize = 1000)
    {
        _processor = processor ?? throw new ArgumentNullException(nameof(processor));
        _batchSize = batchSize;
        _activeSources = new Dictionary<string, ILogSource>();
        _sourcesSemaphore = new SemaphoreSlim(1, 1);

        _channel = Channel.CreateBounded<LogEntry>(new BoundedChannelOptions(batchSize)
        {
            FullMode = BoundedChannelFullMode.DropOldest
        });

        // Start background processing
        _ = ProcessChannelAsync(default);
    }

    public async Task StartCollectingAsync(ILogSource source, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);

        await _sourcesSemaphore.WaitAsync(cancellationToken);
        try
        {
            if (_activeSources.ContainsKey(source.Name))
            {
                throw new InvalidOperationException($"Source {source.Name} is already active");
            }

            await source.InitializeAsync(cancellationToken);
            _activeSources.Add(source.Name, source);

            // Start collecting from source
            _ = CollectFromSourceAsync(source, cancellationToken);
        }
        finally
        {
            _sourcesSemaphore.Release();
        }
    }

    public async Task StopCollectingAsync(string sourceName)
    {
        await _sourcesSemaphore.WaitAsync();
        try
        {
            if (_activeSources.TryGetValue(sourceName, out var source))
            {
                source.Dispose();
                _activeSources.Remove(sourceName);
            }
        }
        finally
        {
            _sourcesSemaphore.Release();
        }
    }

    public async Task<IReadOnlyCollection<string>> GetActiveSourcesAsync()
    {
        await _sourcesSemaphore.WaitAsync();
        try
        {
            return _activeSources.Keys.ToList().AsReadOnly();
        }
        finally
        {
            _sourcesSemaphore.Release();
        }
    }

    public async Task SendLogAsync(LogEntry entry, CancellationToken cancellationToken = default)
    {
        if (!await _channel.Writer.WaitToWriteAsync(cancellationToken))
        {
            throw new InvalidOperationException("Channel cannot accept more writes");
        }

        await _channel.Writer.WriteAsync(entry, cancellationToken);

        await _processor.ProcessLogEntryAsync(entry, cancellationToken);
    }

    private async Task CollectFromSourceAsync(ILogSource source, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Starting to collect from {source.Name}");
        try
        {
            await foreach (var entry in source.GetLogsAsync(cancellationToken))
            {
                if (!await _channel.Writer.WaitToWriteAsync(cancellationToken))
                {
                    Console.WriteLine("Channel cannot accept more writes");
                    break;
                }

                await _channel.Writer.WriteAsync(entry, cancellationToken);
                Console.WriteLine($"Collected log from {source.Name}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error collecting from source {source.Name}: {ex}");
        }
    }

    private async Task ProcessChannelAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Starting channel processing");
        var batch = new List<LogEntry>(_batchSize);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                if (await _channel.Reader.WaitToReadAsync(cancellationToken))
                {
                    while (batch.Count < _batchSize &&
                           _channel.Reader.TryRead(out var entry))
                    {
                        batch.Add(entry);
                    }

                    if (batch.Count > 0)
                    {
                        await _processor.ProcessBatchAsync(batch, cancellationToken);
                        Console.WriteLine($"Processed batch of {batch.Count} logs");
                        batch.Clear();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing batch: {ex}");
                await Task.Delay(1000, cancellationToken);
            }
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            _sourcesSemaphore.Dispose();
            foreach (var source in _activeSources.Values)
            {
                source.Dispose();
            }

            _activeSources.Clear();
        }

        _disposed = true;
    }
}