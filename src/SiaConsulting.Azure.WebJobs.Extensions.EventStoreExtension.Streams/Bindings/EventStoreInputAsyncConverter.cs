using EventStore.ClientAPI;
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
    public class EventStoreInputAsyncConverter : IAsyncConverter<EventStoreStreamsAttribute, IList<ResolvedEvent>>
    {
        private readonly Microsoft.Extensions.Logging.ILogger _logger;

        public EventStoreInputAsyncConverter(Microsoft.Extensions.Logging.ILogger logger)
        {
            _logger = logger;
        }

        public async Task<IList<ResolvedEvent>> ConvertAsync(EventStoreStreamsAttribute config, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(config?.ConnectionStringSetting))
            {
                var esException = new EventStoreStreamsBindingException("ConnectionString cant be empty");
                _logger.LogError(esException, esException.Message);
                throw esException;
            }
            using (var client = new EventStoreClient(config.ConnectionStringSetting, _logger))
            {
                var streamname = GetStreamName(config);
                try
                {
                    await client.Connect();
                    IList<ResolvedEvent> result = null;
                    if (config.StreamReadDirection == StreamReadDirection.Forward)
                    {
                        result = await client.ReadFromStreamForward(streamname, config.StreamOffset, config.ReadSize, config.ResolveLinkTos);
                    }
                    else
                    {
                        result = await client.ReadFromStreamBackward(streamname, config.StreamOffset, config.ReadSize, config.ResolveLinkTos);
                    }
                    return result;
                }
                catch(EventStoreStreamsBindingStreamDeletedException esDeletedException)
                {
                    _logger.LogWarning(esDeletedException, $"Stream with name '{streamname}' is marked deleted");
                    return null;
                }
                catch (EventStoreStreamsBindingStreamNotFoundException esNotFoundException)
                {
                    _logger.LogWarning(esNotFoundException, $"Stream with name '{streamname}' does not exists");
                    return null;
                }
                catch (Exception esException)
                {
                    _logger.LogError(esException, esException.Message);
                    throw;
                }                
            }
        }

        private string GetStreamName(EventStoreStreamsAttribute config) => $"{config.StreamNamePrefix}{config.StreamName}{config.StreamNameSuffix}";
    }
}
