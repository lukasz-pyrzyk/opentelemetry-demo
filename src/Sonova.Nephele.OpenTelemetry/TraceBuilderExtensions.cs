using OpenTelemetry.Trace.Configuration;
using Sonova.Nephele.OpenTelemetry.Implementation;

namespace Sonova.Nephele.OpenTelemetry
{
    public static class TraceBuilderExtensions
    {
        public static TracerBuilder AddServiceBusCollector(this TracerBuilder builder) 
            => builder
                .AddCollector(t => new ServiceBusProcessCollector(t))
                .AddCollector(t => new ServiceBusSendCollector(t));
    }
}
