using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Fibonacci.Shared.ServiceBus;
using Fibonacci.Shared.TableStorage;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Fibonacci.Worker;

class MessageHandler : BackgroundService
{
    private readonly FibonacciTableStorage _repository;
    private readonly IHttpClientFactory _factory;
    private readonly ServiceBusReceiver _processor;
    private readonly ILogger<MessageHandler> _logger;

    public MessageHandler(ServiceBusClient client, FibonacciQueueCfg cfg, FibonacciTableStorage repository, IHttpClientFactory factory, ILogger<MessageHandler> logger)
    {
        _logger = logger;
        _repository = repository;
        _factory = factory;
        _processor = client.CreateReceiver(cfg.EntityPath);
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var msg = await _processor.ReceiveMessageAsync(cancellationToken: ct);
            if (msg is not null)
            {
                await Handle(msg, ct);
            }
        }
    }

    private async Task Handle(ServiceBusReceivedMessage msg, CancellationToken ct)
    {
        var bodyAsString = msg.Body.ToString();
        if (!int.TryParse(bodyAsString, out var n))
        {
            _logger.LogWarning("Received invalid number {bodyAsString}", bodyAsString);
        }

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

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _processor.DisposeAsync();
    }
}