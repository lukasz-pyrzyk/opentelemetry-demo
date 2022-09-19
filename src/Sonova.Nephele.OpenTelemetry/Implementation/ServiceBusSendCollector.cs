using System;
using OpenTelemetry.Collector;
using OpenTelemetry.Trace;

namespace Sonova.Nephele.OpenTelemetry.Implementation
{
    public class ServiceBusSendCollector : IDisposable
    {
        private readonly DiagnosticSourceSubscriber _diagnosticSourceSubscriber;

        public ServiceBusSendCollector(Tracer tracer)
        {
            _diagnosticSourceSubscriber = new DiagnosticSourceSubscriber(new SendMessageListener("Microsoft.Azure.ServiceBus", tracer), null);
            _diagnosticSourceSubscriber.Subscribe();
        }


        public void Dispose() => _diagnosticSourceSubscriber?.Dispose();
    }
}