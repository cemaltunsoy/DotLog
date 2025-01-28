using Collectors;
using Collectors.Sources;
using Core;
using Core.Enums;
using Core.Interfaces;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Processing;

namespace LMediatR.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDotFlowMediator(
        this IServiceCollection services,
        Action<MediatorLoggingOptions>? configureOptions = null)
    {
        // Options konfigürasyonu
        var options = new MediatorLoggingOptions();
        configureOptions?.Invoke(options);
        services.AddSingleton(options);

        // DotFlow servislerinin eklenmesi
        services.AddSingleton<ILogCollector>(sp =>
        {
            var config = new LogSourceConfig
            {
                Name = "MediatorLogger",
                Type = LogSourceType.Console,
                BufferSize = 100
            };

            var source = new ConsoleLogSource(config);
            var processor = new ConsoleLogProcessor();
            var collector = new BufferedLogCollector(processor);
            
            collector.StartCollectingAsync(source).Wait();
            return collector;
        });

        // LMediatR behavior'unun eklenmesi
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

        return services;
    }
}