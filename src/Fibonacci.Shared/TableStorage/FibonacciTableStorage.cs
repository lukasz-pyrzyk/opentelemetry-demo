using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;

namespace Fibonacci.Shared.TableStorage;

public class FibonacciTableStorage
{
    private readonly CloudTable _table;

    public FibonacciTableStorage(FibonacciTableStorageCfg cfg)
    {
        var storageAccount = CloudStorageAccount.Parse(cfg.ConnectionString);
        var tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration
        {
            CosmosExecutorConfiguration = new CosmosExecutorConfiguration
            {
                UseConnectionModeDirect = false
            },
            RestExecutorConfiguration = new RestExecutorConfiguration(),
            UseRestExecutorForCosmosEndpoint = true,
        });

        _table = tableClient.GetTableReference(cfg.TableName);
        _table.CreateIfNotExists();
    }

    public Task<TableResult> Upsert(FibonacciResultEntity entity, CancellationToken ct)
    {
        return _table.ExecuteAsync(TableOperation.InsertOrMerge(entity), ct);
    }

    public async Task Delete(int n, CancellationToken ct)
    {
        var operation = TableOperation.Delete(new TableEntity(FibonacciResultEntity.DefaultPartition, n.ToString()) { ETag = "*" });
        try
        {
            await _table.ExecuteAsync(operation, ct);
        }
        catch (StorageException e) when (e.Message == "Not Found")
        {
        }
    }

    public async Task<int?> GetEntity(int n, CancellationToken ct)
    {
        var operation = TableOperation.Retrieve<FibonacciResultEntity>(FibonacciResultEntity.DefaultPartition, n.ToString());
        var response = await _table.ExecuteAsync(operation, ct);
        if (response.Result is FibonacciResultEntity r)
        {
            return r.Result;
        }

        return null;
    }
}