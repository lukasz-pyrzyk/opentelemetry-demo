using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Sonova.Nephele.OpenTelemetry
{
    public static class ServiceCollectionExtensions
    {
        public static void AddOpenTelemetry(this IServiceCollection services)
        {
            Activity.DefaultIdFormat = ActivityIdFormat.W3C;
        }
    }
}
