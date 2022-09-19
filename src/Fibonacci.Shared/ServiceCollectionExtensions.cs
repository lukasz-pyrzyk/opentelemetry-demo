using System.Diagnostics;
using Azure.Messaging.ServiceBus;
using Fibonacci.Shared.ServiceBus;
using Fibonacci.Shared.TableStorage;
using Fibonacci.WebService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fibonacci.Shared;

public static class ServiceCollectionExtensions
{
    public static void AddOpenTelemetry(this IServiceCollection services)
    {
        Activity.DefaultIdFormat = ActivityIdFormat.W3C;
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