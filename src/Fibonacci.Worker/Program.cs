using Fibonacci.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Fibonacci.Worker;

public class Program
{
    public static void Main(string[] args) => CreateHostBuilder(args).Build().Run();

    private static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
        .UseSerilog((context, logging) =>
        {
            logging.WriteTo.Console();
        })
        .ConfigureServices((ctx, services) =>
        {
            services.AddOpenTelemetry();

            services.AddTableStorage(ctx.Configuration);
            services.AddServiceBusClients(ctx.Configuration);
            services.AddHostedService<MessageHandler>();

        });
}