using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SiaConsulting.Azure.WebJobs.Extensions.EventStoreExtension.Streams.Client;
using SiaConsulting.Azure.WebJobs.Extensions.EventStoreExtension.Streams.Exceptions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SiaConsulting.Azure.WebJobs.Extensions.EventStoreExtension.Streams.Bindings
{
    public class EventStoreDataAsyncCollector : IAsyncCollector<EventStoreData>, IDisposable
    {
        private readonly Dictionary<string, long> _runningTransactions = new Dictionary<string, long>();
        private IEventStoreClient _eventStoreClient;
        private readonly EventStoreStreamsAttribute _config;
        private readonly ILogger _logger;

        public EventStoreDataAsyncCollector(EventStoreStreamsAttribute config, Microsoft.Extensions.Logging.ILogger logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task AddAsync(EventStoreData item, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                if(_eventStoreClient == null)
                {
                    await Connect();
                }
                if(string.IsNullOrWhiteSpace(item.StreamName))
                {
                    item.StreamName = _config.StreamName;
                }
                if (!_runningTransactions.ContainsKey(item.StreamName))
                {
                    var trans = await _eventStoreClient.StartTransaction(item.StreamName, item.ExpectedStreamVersion).ConfigureAwait(false);
                    _runningTransactions.Add(item.StreamName, trans.TransactionId);
                }
                await _eventStoreClient.ContinueTransaction(_runningTransactions[item.StreamName]).WriteAsync(item.Events).ConfigureAwait(false);
            }
            catch(Exception esException)
            {
                _logger.LogError(esException, esException.Message);
                throw;
            }
        }

        public async Task FlushAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                if (_eventStoreClient == null)
                {
                    await Connect();
                }
                var copyTransactions = new Dictionary<string, long>(_runningTransactions);
                foreach (var trans in copyTransactions)
                {
                    var storeTransaction = _eventStoreClient.ContinueTransaction(trans.Value);
                    await storeTransaction.CommitAsync();
                    storeTransaction.Dispose();
                    _runningTransactions.Remove(trans.Key);
                }
            }
            catch(Exception esException)
            {
                _logger.LogError(esException, esException.Message);
                throw;
            }
            finally
            {
                Dispose();
            }
        }

        public void Dispose()
        {
            var copyTransactions = new Dictionary<string, long>(_runningTransactions);
            foreach(var trans in copyTransactions)
            {
                try
                {
                    if (_eventStoreClient != null)
                    {
                        var storeTransaction = _eventStoreClient.ContinueTransaction(trans.Value);
                        storeTransaction.Rollback();
                        storeTransaction.Dispose();
                    }
                }
                catch (Exception esException)
                {
                    _logger.LogError(esException, esException.Message);
                    throw;
                }
                finally
                {
                    _runningTransactions.Remove(trans.Key);
                    _eventStoreClient = null;
                }
            }
        }

        private async Task Connect()
        {
            if (string.IsNullOrWhiteSpace(_config?.ConnectionStringSetting))
            {
                var esException = new EventStoreStreamsBindingException("ConnectionString cant be empty");
                _logger.LogError(esException, esException.Message);
                throw esException;
            }
            try
            {
                _eventStoreClient = new EventStoreClient(_config.ConnectionStringSetting, _logger);
                await _eventStoreClient.Connect().ConfigureAwait(false);
            }
            catch (Exception esException)
            {
                _logger.LogError(esException, esException.Message);
                throw esException;
            }
        }
    }
}
