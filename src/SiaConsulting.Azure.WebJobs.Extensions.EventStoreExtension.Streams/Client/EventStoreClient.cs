using EventStore.ClientAPI;
using Microsoft.Extensions.Logging;
using SiaConsulting.Azure.WebJobs.Extensions.EventStoreExtension.Streams.Exceptions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SiaConsulting.Azure.WebJobs.Extensions.EventStoreExtension.Streams.Client
{
    public class EventStoreClient : IEventStoreClient
    {
        private IEventStoreConnection _client;
        private readonly Microsoft.Extensions.Logging.ILogger _logger;

        public EventStoreClient(string connUri, Microsoft.Extensions.Logging.ILogger logger)
        {
            _logger = logger;
            var settings = ConnectionSettings.Create().UseCustomLogger(new EventStoreClientLogger(_logger)).FailOnNoServerResponse().Build();

            try
            {
                _client = EventStoreConnection.Create(settings, new Uri(connUri));
                _client.AuthenticationFailed += this.Handle_AuthenticationFailed;
                _client.Closed += this.Handle_Closed;
                _client.Connected += this.Handle_Connected;
                _client.Disconnected += this.Handle_Disconnected;
                _client.ErrorOccurred += this.Handle_ErrorOccurred;
                _client.Reconnecting += this.Handle_Reconnecting;
                
            }
            catch (Exception esException)
            {
                logger.LogError(esException, esException.Message);
                throw;
            }
        }

        public event EventHandler<ClientConnectionEventArgs> Connected;
        public event EventHandler<ClientConnectionEventArgs> Disconnected;
        public event EventHandler<ClientAuthenticationFailedEventArgs> AuthenticationFailed;
        public event EventHandler<ClientClosedEventArgs> Closed;
        public event EventHandler<ClientErrorEventArgs> ErrorOccurred;
        public event EventHandler<ClientReconnectingEventArgs> Reconnecting;

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
            return results == null || results.Status != SliceReadStatus.Success ? throw HandleError(results.Status) : events;
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
            return results == null || results.Status != SliceReadStatus.Success ? throw HandleError(results.Status) : events;
        }

        public void Dispose()
        {
            if(_client != null)
            {
                _client.AuthenticationFailed -= this.Handle_AuthenticationFailed;
                _client.Closed -= this.Handle_Closed;
                _client.Connected -= this.Handle_Connected;
                _client.Disconnected -= this.Handle_Disconnected;
                _client.ErrorOccurred -= this.Handle_ErrorOccurred;
                _client.Reconnecting -= this.Handle_Reconnecting;
            }
            _client.Close();
            _client.Dispose();
            _client = null;
        }

        private EventStoreStreamsBindingException HandleError(SliceReadStatus error)
        {
            switch(error)
            {
                case SliceReadStatus.StreamDeleted: return new EventStoreStreamsBindingStreamDeletedException($"Unable to read from Stream: '{nameof(SliceReadStatus.StreamDeleted)}'");
                case SliceReadStatus.StreamNotFound: return new EventStoreStreamsBindingStreamNotFoundException($"Unable to read from Stream: '{nameof(SliceReadStatus.StreamNotFound)}'");
                default: return new EventStoreStreamsBindingException($"This is really an unexpected error!");
            }
        }

        private void Handle_Reconnecting(object sender, ClientReconnectingEventArgs e)
        {
            _logger?.LogTrace($"Reconnecting: {e.Connection.ConnectionName}");
            Reconnecting?.Invoke(sender, e);
        }
        private void Handle_ErrorOccurred(object sender, ClientErrorEventArgs e)
        {
            _logger?.LogError(e.Exception, $"ErrorOccurred: {e.Exception.ToString()}");
            ErrorOccurred?.Invoke(sender, e);
        }
        private void Handle_Disconnected(object sender, ClientConnectionEventArgs e)
        {
            _logger?.LogTrace($"Disconnected from {e.RemoteEndPoint.ToString()}: {e.Connection.ConnectionName}");
            Disconnected?.Invoke(sender, e);
        }
        private void Handle_Connected(object sender, ClientConnectionEventArgs e)
        {
            _logger?.LogTrace($"Connected to {e.RemoteEndPoint.ToString()}: {e.Connection.ConnectionName}");
            Connected?.Invoke(sender, e);
        }
        private void Handle_Closed(object sender, ClientClosedEventArgs e)
        {
            _logger?.LogTrace($"Connection Closed due to {e.Reason}: {e.Connection.ConnectionName}");
            Closed?.Invoke(sender, e);
        }
        private void Handle_AuthenticationFailed(object sender, ClientAuthenticationFailedEventArgs e)
        {
            _logger?.LogError(new EventStoreStreamsBindingException($"Authentication failed due to {e.Reason}"),$"AuthenticationFailed due to {e.Reason}: {e.Connection.ConnectionName}");
            AuthenticationFailed?.Invoke(sender, e);
        }
    }
}
