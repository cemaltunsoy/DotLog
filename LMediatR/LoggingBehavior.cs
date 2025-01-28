using Core;
using Core.Interfaces;
using MediatR;

namespace LMediatR;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogCollector _logCollector;
    private readonly MediatorLoggingOptions _options;

    public LoggingBehavior(ILogCollector logCollector, MediatorLoggingOptions options)
    {
        _logCollector = logCollector;
        _options = options;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestType = request.GetType();
        var typeName = requestType.Name;
        var startTime = DateTime.UtcNow;
        
        try 
        {
            var response = await next();
            var executionTime = DateTime.UtcNow - startTime;

            // Execution time'a göre log level belirleme
            var level = executionTime.TotalMilliseconds > _options.MaxAcceptableExecutionTime 
                ? "Warning" 
                : "Information";

            var log = new LogEntry
            {
                Timestamp = DateTime.UtcNow,
                Level = level,
                Message = $"{typeName} - {executionTime.TotalMilliseconds:F2}ms",
                Source = "Mediator",
                ClassName = typeName,
                ExecutionTime = executionTime,
                Properties = new Dictionary<string, object>
                {
                    ["RequestType"] = requestType.FullName!,
                    ["ExecutionTimeMs"] = executionTime.TotalMilliseconds,
                    ["Namespace"] = requestType.Namespace ?? "Unknown"
                }
            };

            await _logCollector.SendLogAsync(log, cancellationToken);
            
            return response;
        }
        catch (Exception ex)
        {
            var executionTime = DateTime.UtcNow - startTime;
            
            var log = new LogEntry
            {
                Timestamp = DateTime.UtcNow,
                Level = "Error",
                Message = $"Error in {typeName} - {executionTime.TotalMilliseconds:F2}ms",
                Source = "Mediator",
                ClassName = typeName,
                ExecutionTime = executionTime,
                Exception = ex.Message,
                StackTrace = ex.StackTrace,
                Properties = new Dictionary<string, object>
                {
                    ["RequestType"] = requestType.FullName!,
                    ["ExecutionTimeMs"] = executionTime.TotalMilliseconds,
                    ["Namespace"] = requestType.Namespace ?? "Unknown",
                    ["ExceptionType"] = ex.GetType().Name,
                    ["ExceptionLocation"] = ex.StackTrace?.Split('\n').FirstOrDefault() ?? "Unknown"
                }
            };

            await _logCollector.SendLogAsync(log, cancellationToken);
            throw;
        }
    }
}