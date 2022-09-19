using System.Diagnostics;
using Microsoft.Azure.ServiceBus;

namespace Sonova.Nephele.OpenTelemetry
{
    public static class MessageExtensions
    {
        public static void EnrichWithTelemetry(this Message msg)
        {
            var activity = Activity.Current;
            msg.UserProperties.Add("traceId", activity.TraceId.ToString());
            msg.UserProperties.Add("parentSpanId", activity.SpanId.ToString());
        }

        public static string? GetTraceId(this Message msg)
        {
            return msg.UserProperties["traceId"] as string;
        }

        public static string? GetParentSpanId(this Message msg)
        {
            return msg.UserProperties["parentSpanId"] as string;
        }
    }
}