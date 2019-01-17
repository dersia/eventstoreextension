using EventStore.ClientAPI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SiaConsulting.Azure.WebJobs.Extensions.EventStoreExtension.Streams.Client
{
    public interface IEventStoreClient : IDisposable
    {
        Task Connect();
        Task AppendToStream(string streamName, IEnumerable<EventData> events);
        Task AppendToStream(string streamName, long expectedVersion, IEnumerable<EventData> events);
        Task AppendToStream(string streamName, params EventData[] events);
        Task AppendToStream(string streamName, long expectedVersion, params EventData[] events);
        Task<EventStoreTransaction> StartTransaction(string streamName, long expectedVersion = ExpectedVersion.Any);
        EventStoreTransaction ContinueTransaction(long transactionId);
        Task<EventReadResult> ReadEventFromStream(string streamName, long eventNumber, bool resolveLinkTos);
        Task<IList<ResolvedEvent>> ReadFromStreamForward(string streamName, long start, int count, bool resolveLinkTos);
        Task<IList<ResolvedEvent>> ReadFromStreamBackward(string streamName, long start, int count, bool resolveLinkTos);
    }
}
