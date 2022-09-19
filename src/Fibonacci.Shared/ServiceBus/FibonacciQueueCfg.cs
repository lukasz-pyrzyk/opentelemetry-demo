namespace Fibonacci.Shared.ServiceBus;

public class FibonacciQueueCfg
{
    public string ConnectionString { get; set; }
    public string EntityPath { get; set; }
}