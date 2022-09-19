using System;
using System.Diagnostics;
using OpenTelemetry.Collector;
using OpenTelemetry.Trace;

namespace Sonova.Nephele.OpenTelemetry.Implementation
{
    internal class SendMessageListener : ListenerHandler
    {
        private const string OperationFilter = "Microsoft.Azure.ServiceBus.Send";

        public SendMessageListener(string sourceName, Tracer tracer) : base(sourceName, tracer)
        {
        }

        public override void OnStartActivity(Activity activity, object payload)
        {
            if (payload is null)
            {
                CollectorEventSource.Log.NullPayload($"{nameof(SendMessageListener)}.{nameof(OnStartActivity)}");
                return;
            }

            if (activity.OperationName == OperationFilter)
            {
                var endpoint = payload.GetEndpoint();
                var entityPath = payload.GetEntityPath();
                var messages = payload.GetMessages();
                var operationName = $"Sending msg to {endpoint}/{entityPath}";

                Tracer.StartActiveSpanFromActivity(operationName, activity, SpanKind.Producer, out var span);

                if (span.IsRecording)
                {
                    span.SetAttribute("entityPath", entityPath);
                    span.SetAttribute("messagesCount", messages.Count);
                }
            }
        }

        public override void OnStopActivity(Activity activity, object payload)
        {
            if (activity.OperationName == OperationFilter && Tracer.CurrentSpan.Context.IsValid)
            {
                Tracer.CurrentSpan.End();
            }

            base.OnStopActivity(activity, payload);
        }
    }
}