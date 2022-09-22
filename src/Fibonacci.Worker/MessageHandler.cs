using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Fibonacci.Shared;
using Fibonacci.Shared.ServiceBus;
using Fibonacci.Shared.TableStorage;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Fibonacci.Worker;

class MessageHandler : BackgroundService
{
    private static ActivitySource ActivitySource { get; } = new(Settings.CalculationActivityName);
    private readonly FibonacciTableStorage _repository;
    private readonly ServiceBusProcessor _processor;
    private readonly ILogger<MessageHandler> _logger;

    public MessageHandler(ServiceBusClient client, FibonacciQueueCfg cfg, FibonacciTableStorage repository, ILogger<MessageHandler> logger)
    {
        _logger = logger;
        _repository = repository;
        _processor = client.CreateProcessor(cfg.EntityPath);
        _processor.ProcessErrorAsync += ProcessorOnProcessErrorAsync;
        _processor.ProcessMessageAsync += ProcessorOnProcessMessageAsync;
    }

    private Task ProcessorOnProcessErrorAsync(ProcessErrorEventArgs arg)
    {
        _logger.LogError(arg.Exception, "Received exception when reading servicebus message");
        return Task.CompletedTask;
    }

    private async Task ProcessorOnProcessMessageAsync(ProcessMessageEventArgs arg)
    {
        var bodyAsString = arg.Message.Body.ToString();
        if (!int.TryParse(bodyAsString, out var n))
        {
            _logger.LogWarning("Received invalid number {bodyAsString}", bodyAsString);
            return;
        }

        var fib = Fibonacci(n);
        _logger.LogInformation("Calculated fib value {fib} for n {n}.", fib, n);

        await _repository.Upsert(new FibonacciResultEntity(n, fib), arg.CancellationToken);
        _logger.LogInformation("Saved result for n {n}", n);
    }

    private int Fibonacci(int n)
    {
        using var activity = ActivitySource.StartActivity("Calculating fibonacci");

        Thread.Sleep(n * 100); // yea, I know

        int a = 0; int b = 1;
        for (var i = 0; i < n; i++)
        {
            var temp = a;
            a = b;
            b = temp + b;
        }

        activity?.SetTag("Value", n);
        activity?.SetTag("Result", a);

        return a;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        await _processor.StartProcessingAsync(ct);
    }

    public override async Task StopAsync(CancellationToken ct)
    {
        await _processor.StopProcessingAsync(ct);
        await _processor.DisposeAsync();
    }
}