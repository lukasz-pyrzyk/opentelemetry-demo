using Fibonacci.Shared;
using Fibonacci.Shared.TableStorage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fibonacci.Worker;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; set; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddOpenTelemetry();

        services.AddHttpClient();
        
        services.AddTableStorage(Configuration);
        services.AddServiceBusClients(Configuration);
        services.AddHostedService<MessageHandler>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
    }   
}