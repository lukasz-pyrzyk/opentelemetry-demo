using System;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace.Configuration;

namespace Sonova.Nephele.OpenTelemetry
{
    public static class ServiceCollectionExtensions
    {
        public static void AddDistributedTelemetry(this IServiceCollection services)
        {
            Activity.DefaultIdFormat = ActivityIdFormat.W3C;

            var applicationName = Assembly.GetEntryAssembly()?.GetName().Name ?? "unknown";
            services.AddOpenTelemetry(builder =>
            {
                builder
                    .UseZipkin(x =>
                    {
                        x.Endpoint = new Uri("http://localhost:9411/api/v2/spans");
                    })
                    .UseApplicationInsights(x => { x.InstrumentationKey = "1d03ce05-c251-4792-a44e-e3c0e07f5b97"; })
                    .AddRequestCollector() // incoming requests
                    .AddDependencyCollector(configureSqlCollectorOptions: options =>
                        {
                            options.CaptureStoredProcedureCommandName = true;
                            options.CaptureTextCommandContent = true;
                        }) // outgoing requests
                    .AddServiceBusCollector() // incoming and outgoing messages
                    .SetResource(Resources.CreateServiceResource(applicationName));
            });

        }
    }
}
