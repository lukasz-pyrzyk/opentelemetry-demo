using System;
using Azure;
using Azure.Data.Tables;

namespace Fibonacci.Shared.TableStorage;

public class FibonacciResultEntity : ITableEntity
{
    // ReSharper disable once UnusedMember.Global
    public FibonacciResultEntity()
    {
    }

    public FibonacciResultEntity(int n, int result)
    {
        PartitionKey = DefaultPartition;
        RowKey = n.ToString();
        Result = result;
    }

    public int Result { get; set; }

    public static string DefaultPartition = "partition";

    public string PartitionKey { get; set; }

    public string RowKey { get; set; }

    public DateTimeOffset? Timestamp { get; set; }

    public ETag ETag { get; set; }
}