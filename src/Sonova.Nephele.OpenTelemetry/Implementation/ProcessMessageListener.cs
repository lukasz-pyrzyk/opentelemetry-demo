using System;
using System.Diagnostics;
using Microsoft.Azure.ServiceBus;
using OpenTelemetry.Collector;
using OpenTelemetry.Trace;

namespace Sonova.Nephele.OpenTelemetry.Implementation
{
    internal class ProcessMessageListener : ListenerHandler
    {
        private const string OperationFilter = "Microsoft.Azure.ServiceBus.Process";

        public ProcessMessageListener(string sourceName, Tracer tracer) : base(sourceName, tracer)
        {
        }

        public override void OnStartActivity(Activity activity, object payload)
        {
            if (payload == null)
            {
                CollectorEventSource.Log.NullPayload("ProcessMessageListener.OnStartActivity");
                return;
            }

            if (activity.OperationName == OperationFilter)
            {
                if (payload.GetType().GetProperty("Message")?.GetValue(payload) is Message msg)
                {
                    var rawTraceId = msg.GetTraceId();
                    var rawParentSpanId = msg.GetParentSpanId();

                    var traceId = !string.IsNullOrEmpty(rawTraceId) ? ActivityTraceId.CreateFromString(rawTraceId) : ActivityTraceId.CreateRandom();
                    var parentSpanId = !string.IsNullOrEmpty(rawParentSpanId) ? ActivitySpanId.CreateFromString(rawParentSpanId) : ActivitySpanId.CreateRandom();

                    var currentActivity = activity.SetParentId(traceId, parentSpanId).SetStartTime(DateTime.UtcNow);
                    var endpoint = payload.GetEndpoint();
                    var entityPath = payload.GetEntityPath();
                    var operationName = $"Processing msg from {endpoint}/{entityPath}";

                    Tracer.StartActiveSpanFromActivity(operationName, currentActivity, SpanKind.Consumer, out var span);

                    if (span.IsRecording)
                    {
                        span.SetAttribute("entityPath", entityPath);
                        foreach (var pair in msg.UserProperties)
                        {
                            span.SetAttribute(pair.Key, pair.Value);
                        }
                    }
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