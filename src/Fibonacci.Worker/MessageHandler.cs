using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Fibonacci.Shared;
using Fibonacci.Shared.Cfg;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Fibonacci.Worker
{
    class MessageHandler : BackgroundService
    {
        private readonly Repository _repository;
        private readonly IHttpClientFactory _factory;
        private readonly QueueClient _queueClient;
        private readonly ILogger<MessageHandler> _logger;

        public MessageHandler(QueueCfg cfg, Repository repository, IHttpClientFactory factory, ILogger<MessageHandler> logger)
        {
            _logger = logger;
            _repository = repository;
            _factory = factory;
            _queueClient = new QueueClient(cfg.ConnectionString, cfg.EntityPath);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var options = new MessageHandlerOptions(ExceptionHandler)
            {
                MaxConcurrentCalls = 1,
                AutoComplete = true,
            };

            _queueClient.RegisterMessageHandler(Handle, options);

            return Task.CompletedTask;
        }

        private async Task Handle(Message msg, CancellationToken ct)
        {
            using var _ = _logger.BeginScope("{TraceId}, {ParentId}, {SpanId}", Activity.Current.TraceId, Activity.Current.ParentSpanId, Activity.Current.SpanId);

            var n = BitConverter.ToInt32(msg.Body);
            var fib = Fibonacci(n);
            _logger.LogInformation("Calculated fib value {fib} for n {n}.", fib, n);

            await _repository.Upsert(new FibonacciResultEntity(n, fib), ct);
            _logger.LogInformation("Saved result for n {n}", n);

            // dummy call to check if activity data is added to outgoing requests
            await CallExternalService();

        }

        private async Task CallExternalService()
        {
            var client = _factory.CreateClient();
            using var _ = await client.GetAsync("https://google.com");
        }

        private Task ExceptionHandler(ExceptionReceivedEventArgs arg)
        {
            _logger.LogError(arg.Exception, "Received new error message");
            return Task.CompletedTask;
        }

        private static int Fibonacci(int n)
        {
            int a = 0; int b = 1;
            for (var i = 0; i < n; i++)
            {
                var temp = a;
                a = b;
                b = temp + b;
            }
            return a;
        }

        public override void Dispose()
        {
            _queueClient.CloseAsync().GetAwaiter().GetResult();
            base.Dispose();
        }
    }
}
