using Collectors;
using Collectors.Sources;
using Core;
using Core.Enums;
using Processing;

var config = new LogSourceConfig
{
    Name = "ConsoleSource",
    Type = LogSourceType.Console,
    BufferSize = 100,
    FlushInterval = TimeSpan.FromSeconds(1)
};

try
{
    using var cts = new CancellationTokenSource();
    using var source = new ConsoleLogSource(config);
    using var collector = new BufferedLogCollector(new ConsoleLogProcessor(), batchSize: 5);

    Console.WriteLine("Press any key to stop...");

    // Start collecting
    await collector.StartCollectingAsync(source, cts.Token);

    // Wait for key press to stop
    Console.ReadKey();
            
    // Cancel token and cleanup
    cts.Cancel();
    await collector.StopCollectingAsync(source.Name);

    Console.WriteLine("Stopped collecting logs.");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine($"StackTrace: {ex.StackTrace}");
}