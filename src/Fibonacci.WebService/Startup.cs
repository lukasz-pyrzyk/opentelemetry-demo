using Fibonacci.Shared;
using Fibonacci.Shared.HistoryDatabase;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Fibonacci.WebService;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }


    public void ConfigureServices(IServiceCollection services)
    {
        services.AddOpenTelemetry("WebService");
        
        services.AddTableStorage(Configuration);
        services.AddServiceBusClients(Configuration);

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