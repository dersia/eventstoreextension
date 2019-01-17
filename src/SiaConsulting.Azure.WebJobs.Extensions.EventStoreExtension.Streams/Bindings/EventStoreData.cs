using EventStore.ClientAPI;
using System.Collections.Generic;

namespace SiaConsulting.Azure.WebJobs.Extensions.EventStoreExtension.Streams.Bindings
{
    public class EventStoreData
    {
        public string StreamName { get; set; }
        public IEnumerable<EventData> Events { get; set; }
        public long ExpectedStreamVersion { get; set; } = ExpectedVersion.Any;
    }
}
