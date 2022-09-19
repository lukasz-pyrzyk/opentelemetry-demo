using System.Threading;
using System.Threading.Tasks;
using Azure.Data.Tables;

namespace Fibonacci.Shared.TableStorage;

public class FibonacciTableStorage
{
    private readonly TableClient _table;

    public FibonacciTableStorage(FibonacciTableStorageCfg cfg)
    {
        _table = new TableClient(cfg.ConnectionString, cfg.TableName);
        _table.CreateIfNotExists();
    }

    public async Task Upsert(FibonacciResultEntity entity, CancellationToken ct)
    {
        await _table.UpsertEntityAsync(entity, cancellationToken: ct);
    }

    public async Task Delete(int n, CancellationToken ct)
    {
        await _table.DeleteEntityAsync(FibonacciResultEntity.DefaultPartition, n.ToString(), cancellationToken: ct);
    }

    public async Task<int?> Get(int n, CancellationToken ct)
    {
        var entity = await _table.GetEntityAsync<FibonacciResultEntity>(FibonacciResultEntity.DefaultPartition, n.ToString(), cancellationToken: ct);
        return entity?.Value.Result;
    }
}