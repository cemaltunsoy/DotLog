using Core.Enums;

namespace Core;

public class LogSourceConfig
{
    public string Name { get; set; } = string.Empty;
    public LogSourceType Type { get; set; }
    public Dictionary<string, string> ConnectionSettings { get; set; } = new();
    public int BufferSize { get; set; } = 1000;
    public TimeSpan FlushInterval { get; set; } = TimeSpan.FromSeconds(5);
}