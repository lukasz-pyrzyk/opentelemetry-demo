using Microsoft.Azure.Cosmos.Table;

namespace Fibonacci.Shared;

public class FibonacciResultEntity : TableEntity
{
    // ReSharper disable once UnusedMember.Global
    public FibonacciResultEntity()
    {
    }

    public FibonacciResultEntity(int n, int result) : base(DefaultPartition, n.ToString())
    {
        Result = result;
    }

    public int Result { get; set; }

    public static string DefaultPartition = "partition";
}