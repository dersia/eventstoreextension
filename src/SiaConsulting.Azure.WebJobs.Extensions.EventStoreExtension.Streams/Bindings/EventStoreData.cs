using EventStore.ClientAPI;
using System.Collections.Generic;
using System.Linq;

namespace SiaConsulting.Azure.WebJobs.Extensions.EventStoreExtension.Streams.Bindings
{
    public class EventStoreData
    {
        public EventStoreData() { }
        public EventStoreData(string streamName, long expectedStreamVersion = ExpectedVersion.Any, params EventData[] events)
        {
            StreamName = streamName;
            Events = events;
            ExpectedStreamVersion = expectedStreamVersion;
        }

        public EventStoreData(string streamName, long expectedStreamVersion = ExpectedVersion.Any, IEnumerable<EventData> events = null)
            : this(streamName, expectedStreamVersion, (events ?? new List<EventData>()).ToArray()) { }

        public string StreamName { get; set; }
        public IEnumerable<EventData> Events { get; set; }
        public long ExpectedStreamVersion { get; set; } = ExpectedVersion.Any;
    }
}
