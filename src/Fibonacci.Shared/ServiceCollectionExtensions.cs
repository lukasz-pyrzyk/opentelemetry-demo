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
        //using ActivityListener listener = new ActivityListener()
        //{
        //    ShouldListenTo = a => a.Name.StartsWith("Azure"),
        //    Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
        //    SampleUsingParentId = (ref ActivityCreationOptions<string> _) => ActivitySamplingResult.AllData,
        //    ActivityStarted = activity => Console.WriteLine("Start: " + activity.DisplayName),
        //    ActivityStopped = activity => Console.WriteLine("Stop: " + activity.DisplayName)
        //};
        //ActivitySource.AddActivityListener(listener);

        services.AddOpenTelemetryTracing(builder => builder
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
            .AddZipkinExporter(x => x.Endpoint = new Uri("http://localhost:9411/api/v2/spans")));

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