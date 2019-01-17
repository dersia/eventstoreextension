using EventStore.ClientAPI;
using Microsoft.Azure.WebJobs;
using SiaConsulting.Azure.WebJobs.Extensions.EventStoreExtension.Streams.Client;
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
            using (var client = new EventStoreClient(config.ConnectionStringSetting, _logger))
            {
                await client.Connect();
                IList<ResolvedEvent> result = null;
                if (config.StreamReadDirection == StreamReadDirection.Forward)
                {
                    result = await client.ReadFromStreamForward(config.StreamName, config.StreamOffset, config.ReadSize, config.ResolveLinkTos);
                }
                else
                {
                    result = await client.ReadFromStreamBackward(config.StreamName, config.StreamOffset, config.ReadSize, config.ResolveLinkTos);
                }
                return result;
            }
        }
    }
}
