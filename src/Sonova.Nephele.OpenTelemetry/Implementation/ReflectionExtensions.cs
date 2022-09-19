using System;
using System.Collections.Generic;
using Microsoft.Azure.ServiceBus;

namespace Sonova.Nephele.OpenTelemetry.Implementation
{
    internal static class ReflectionExtensions
    {
        public static string GetEntityPath(this object payload)
        {
            return payload.GetType().GetProperty("Entity")?.GetValue(payload) as string;
        }

        public static Uri GetEndpoint(this object payload)
        {
            return payload.GetType().GetProperty("Endpoint")?.GetValue(payload) as Uri;
        }

        public static IList<Message> GetMessages(this object payload)
        {
            return payload.GetType().GetProperty("Messages")?.GetValue(payload) as IList<Message>;
        }
    }
}
