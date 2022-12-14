using System;
using System.Diagnostics;
using Azure.Messaging.ServiceBus;
using Fibonacci.Shared.ServiceBus;
using Fibonacci.Shared.TableStorage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Fibonacci.Shared;

public static class ServiceCollectionExtensions
{
    public static void AddOpenTelemetry(this IServiceCollection services, string serviceName)
    {
        Activity.DefaultIdFormat = ActivityIdFormat.W3C;

        AppContext.SetSwitch("Azure.Experimental.EnableActivitySource", true);

        services
            .AddOpenTelemetryTracing(builder => builder
                .AddSource("Azure.*")
                .AddSource(Settings.CalculationActivityName)
                .AddAspNetCoreInstrumentation()
                .AddEntityFrameworkCoreInstrumentation(o =>
                {
                    o.SetDbStatementForText = true;
                    o.SetDbStatementForStoredProcedure = true;
                })
                .AddHttpClientInstrumentation()
                .SetResourceBuilder(ResourceBuilder.CreateDefault()
                    .AddService(serviceName, "Fibonacci")
                    .AddTelemetrySdk())
                .AddZipkinExporter()
               //.AddJaegerExporter()
            );
    }

    public static void AddServiceBusClients(this IServiceCollection services, IConfiguration configuration)
    {
        var cfg = new FibonacciQueueCfg();
        configuration.GetSection("Queue").Bind(cfg);
        services.AddSingleton(cfg);
        services.AddSingleton(new ServiceBusClient(cfg.ConnectionString));
        services.AddSingleton<FibonacciQueueSender>();
    }

    public static void AddTableStorage(this IServiceCollection services, IConfiguration configuration)
    {
        var tableCfg = new FibonacciTableStorageCfg();
        configuration.GetSection("TableStorage").Bind(tableCfg);
        services.AddSingleton(tableCfg);
        services.AddSingleton<FibonacciTableStorage>();
    }
}