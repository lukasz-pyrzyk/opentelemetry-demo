using System;
using OpenTelemetry.Collector;
using OpenTelemetry.Trace;

namespace Sonova.Nephele.OpenTelemetry.Implementation
{
    public class ServiceBusProcessCollector : IDisposable
    {
        private readonly DiagnosticSourceSubscriber _diagnosticSourceSubscriber;

        public ServiceBusProcessCollector(Tracer tracer)
        {
            _diagnosticSourceSubscriber = new DiagnosticSourceSubscriber(new ProcessMessageListener("Microsoft.Azure.ServiceBus", tracer), null);
            _diagnosticSourceSubscriber.Subscribe();
        }


        public void Dispose() => _diagnosticSourceSubscriber?.Dispose();
    }
}