using Core;
using Core.Interfaces;

namespace Processing;

public class ConsoleLogProcessor : ILogProcessor
{
    public Task ProcessLogEntryAsync(LogEntry entry, CancellationToken cancellationToken = default)
    {
        var color = entry.Level switch
        {
            "Error" => ConsoleColor.Red,
            "Warning" => ConsoleColor.Yellow,
            "Information" => ConsoleColor.Green,
            _ => ConsoleColor.White
        };

        var originalColor = Console.ForegroundColor;
        Console.ForegroundColor = color;

        if (entry.Level == "Error")
        {
            Console.WriteLine($"ERROR IN {entry.ClassName}");
            Console.WriteLine($"Location: {entry.Properties["ExceptionLocation"]}");
            Console.WriteLine($"Message: {entry.Exception}");
            Console.WriteLine($"Execution Time: {entry.ExecutionTime?.TotalMilliseconds:F2}ms");
        }
        else
        {
            Console.WriteLine($"{entry.ClassName} - {entry.ExecutionTime?.TotalMilliseconds:F2}ms");
        }

        Console.ForegroundColor = originalColor;
        
        return Task.CompletedTask;
    }

    public Task ProcessBatchAsync(IEnumerable<LogEntry> entries, CancellationToken cancellationToken = default)
    {
        foreach (var entry in entries)
        {
            ProcessLogEntryAsync(entry, cancellationToken).Wait(cancellationToken);
        }
        return Task.CompletedTask;
    }
}