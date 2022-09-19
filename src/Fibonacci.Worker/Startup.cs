using Fibonacci.Shared;
using Fibonacci.Shared.Cfg;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sonova.Nephele.OpenTelemetry;

namespace Fibonacci.Worker
{
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

            var tableCfg = new TableStorageCfg();
            Configuration.GetSection("TableStorage").Bind(tableCfg);
            services.AddSingleton(tableCfg);
            services.AddSingleton<Repository>();

            var queueCfg = new QueueCfg();
            Configuration.GetSection("Queue").Bind(queueCfg);
            services.AddSingleton(queueCfg);
            services.AddHostedService<MessageHandler>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
        }   
    }
}
