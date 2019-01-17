using EventStore.ClientAPI;
using SiaConsulting.Azure.WebJobs.Extensions.EventStoreExtension.Streams.Exceptions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SiaConsulting.Azure.WebJobs.Extensions.EventStoreExtension.Streams.Client
{
    public class EventStoreClient : IEventStoreClient
    {
        private readonly IEventStoreConnection _client;
        private readonly Microsoft.Extensions.Logging.ILogger _logger;

        public EventStoreClient(string connUri, Microsoft.Extensions.Logging.ILogger logger)
        {
            var settings = ConnectionSettings.Create().UseCustomLogger(new EventStoreClientLogger(logger)).FailOnNoServerResponse().Build();
            
            _client = EventStoreConnection.Create(settings,new Uri(connUri));
        }

        public Task Connect()
            => _client.ConnectAsync(); 

        public Task AppendToStream(string streamName, IEnumerable<EventData> events) 
            => _client.AppendToStreamAsync(streamName, ExpectedVersion.Any, events);

        public Task AppendToStream(string streamName, long expectedVersion, IEnumerable<EventData> events) 
            => _client.AppendToStreamAsync(streamName, expectedVersion, events);

        public Task AppendToStream(string streamName, params EventData[] events)
            => _client.AppendToStreamAsync(streamName, ExpectedVersion.Any, events);

        public Task AppendToStream(string streamName, long expectedVersion, params EventData[] events)
            => _client.AppendToStreamAsync(streamName, expectedVersion, events);

        public Task<EventStoreTransaction> StartTransaction(string streamName, long expectedVersion = ExpectedVersion.Any)
            => _client.StartTransactionAsync(streamName, expectedVersion);
        public EventStoreTransaction ContinueTransaction(long transactionId) 
            => _client.ContinueTransaction(transactionId);

        public Task<EventReadResult> ReadEventFromStream(string streamName, long eventNumber, bool resolveLinkTos)
            => _client.ReadEventAsync(streamName, eventNumber, resolveLinkTos);

        public async Task<IList<ResolvedEvent>> ReadFromStreamForward(string streamName, long start, int count, bool resolveLinkTos)
        {
            var events = new List<ResolvedEvent>();
            StreamEventsSlice results = null;
            do
            {
                results = await Read(streamName, results != null ? results.NextEventNumber : start, count, resolveLinkTos);
                if(results.Status == SliceReadStatus.Success)
                {
                    events.AddRange(results.Events);
                }
            }
            while (count < 0 && results != null && results.Status == SliceReadStatus.Success && !results.IsEndOfStream);

            async Task<StreamEventsSlice> Read(string readStreamName, long readStart, int readCount, bool readResolveLinkTos)
            {
                return await _client.ReadStreamEventsForwardAsync(readStreamName, readStart == -1 ? StreamPosition.Start : readStart, readCount == -1 ? 1000 : readCount, readResolveLinkTos);
            }
            return results == null || results.Status != SliceReadStatus.Success ? throw new EventStoreStreamsBindingException($"Unable to read from Stream: '{results.Status}'") : events;
        }

        public async Task<IList<ResolvedEvent>> ReadFromStreamBackward(string streamName, long start, int count, bool resolveLinkTos)
        {
            var events = new List<ResolvedEvent>();
            StreamEventsSlice results = null;
            do
            {
                results = await Read(streamName, results != null ? results.NextEventNumber : start, count, resolveLinkTos);
                if (results.Status == SliceReadStatus.Success)
                {
                    events.AddRange(results.Events);
                }
            }
            while (count < 0 && results != null && results.Status == SliceReadStatus.Success && !results.IsEndOfStream);

            async Task<StreamEventsSlice> Read(string readStreamName, long readStart, int readCount, bool readResolveLinkTos)
            {                
                return await _client.ReadStreamEventsBackwardAsync(readStreamName, readStart == -1 ? StreamPosition.End : readStart, readCount == -1 ? 1000 : readCount, readResolveLinkTos);
            }
            return results == null || results.Status != SliceReadStatus.Success ? throw new EventStoreStreamsBindingException($"Unable to read from Stream: '{results.Status}'") : events;
        }

        public void Dispose()
        {
            _client.Close();
            _client.Dispose();
        }
    }
}
