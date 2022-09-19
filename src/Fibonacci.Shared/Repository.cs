using System.Threading;
using System.Threading.Tasks;
using Fibonacci.Shared.Cfg;
using Microsoft.Azure.Cosmos.Table;

namespace Fibonacci.Shared
{
    public class Repository
    {
        private readonly CloudTable _table;

        public Repository(TableStorageCfg cfg)
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

        public async Task Upsert(FibonacciResultEntity entity, CancellationToken ct)
        {
            _ = await _table.ExecuteAsync(TableOperation.InsertOrMerge(entity), ct);
        }

        public async Task Delete(int n, CancellationToken ct)
        {
            var operation = TableOperation.Delete(new TableEntity(FibonacciResultEntity.DefaultPartition, n.ToString()) { ETag = "*" });
            try
            {
                await _table.ExecuteAsync(operation, ct);
            }
            catch (StorageException)
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
}
