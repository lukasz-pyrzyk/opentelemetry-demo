using Fibonacci.Shared;
using Fibonacci.Shared.Cfg;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sonova.Nephele.OpenTelemetry;

namespace Fibonacci.WebService
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
            services.AddDistributedTelemetry();

            var tableCfg = new TableStorageCfg();
            Configuration.GetSection("TableStorage").Bind(tableCfg);
            services.AddSingleton(tableCfg);
            services.AddSingleton<Repository>();

            var cfg = new QueueCfg();
            Configuration.GetSection("Queue").Bind(cfg);
            services.AddSingleton(cfg);
            services.AddSingleton(new QueueClient(cfg.ConnectionString, cfg.EntityPath));

            services.AddDbContextPool<HistoryDbContext>(builder =>
            {
                var cs = Configuration.GetConnectionString("Db");
                builder.UseSqlServer(cs);
            });

            services.AddControllers();
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseEndpoints(endpoints => { endpoints.MapDefaultControllerRoute(); });
        }
    }
}
