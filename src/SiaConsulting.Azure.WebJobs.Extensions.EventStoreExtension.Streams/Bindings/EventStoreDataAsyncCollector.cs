using Microsoft.Azure.WebJobs;
using SiaConsulting.Azure.WebJobs.Extensions.EventStoreExtension.Streams.Client;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SiaConsulting.Azure.WebJobs.Extensions.EventStoreExtension.Streams.Bindings
{
    public class EventStoreDataAsyncCollector : IAsyncCollector<EventStoreData>, IDisposable
    {
        private readonly Dictionary<string, long> _runningTransactions = new Dictionary<string, long>();
        private readonly IEventStoreClient _eventStoreClient;

        public EventStoreDataAsyncCollector(EventStoreStreamsAttribute config, Microsoft.Extensions.Logging.ILogger logger)
        {
            _eventStoreClient = new EventStoreClient(config.ConnectionStringSetting, logger);
            _eventStoreClient.Connect().Wait();
        }

        public async Task AddAsync(EventStoreData item, CancellationToken cancellationToken = default(CancellationToken))
        {
            if(!_runningTransactions.ContainsKey(item.StreamName))
            {
                var trans = await _eventStoreClient.StartTransaction(item.StreamName, item.ExpectedStreamVersion).ConfigureAwait(false);
                _runningTransactions.Add(item.StreamName, trans.TransactionId);
            }
            await _eventStoreClient.ContinueTransaction(_runningTransactions[item.StreamName]).WriteAsync(item.Events).ConfigureAwait(false);
        }

        public async Task FlushAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var copyTransactions = new Dictionary<string, long>(_runningTransactions);
            foreach (var trans in copyTransactions)
            {
                var storeTransaction = _eventStoreClient.ContinueTransaction(trans.Value);
                await storeTransaction.CommitAsync();
                storeTransaction.Dispose();
                _runningTransactions.Remove(trans.Key);
            }
            Dispose();
        }

        public void Dispose()
        {
            var copyTransactions = new Dictionary<string, long>(_runningTransactions);
            foreach(var trans in copyTransactions)
            {
                var storeTransaction = _eventStoreClient.ContinueTransaction(trans.Value);
                storeTransaction.Rollback();
                storeTransaction.Dispose();
                _runningTransactions.Remove(trans.Key);
            }
        }
    }
}
