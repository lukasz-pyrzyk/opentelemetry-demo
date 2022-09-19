using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

namespace Fibonacci.Shared.ServiceBus
{
    public class FibonacciQueueSender : IAsyncDisposable
    {
        private readonly ServiceBusSender _sender;

        public FibonacciQueueSender(ServiceBusClient client, FibonacciQueueCfg cfg)
        {
            _sender = client.CreateSender(cfg.EntityPath);
        }

        public Task Send(ServiceBusMessage msg) => _sender.SendMessageAsync(msg);

        public ValueTask DisposeAsync() => _sender.DisposeAsync();
    }
}
