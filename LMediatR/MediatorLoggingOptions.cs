namespace LMediatR;

public class MediatorLoggingOptions
{
    public int MaxAcceptableExecutionTime { get; set; } = 1000; // Default 1 saniye
    public bool IncludeExceptionDetails { get; set; } = true;
}