using System;
using System.Runtime.Serialization;

namespace SiaConsulting.Azure.WebJobs.Extensions.EventStoreExtension.Streams.Exceptions
{
    public class EventStoreStreamsBindingStreamDeletedException : EventStoreStreamsBindingException
    {
        public EventStoreStreamsBindingStreamDeletedException()
        {
        }

        public EventStoreStreamsBindingStreamDeletedException(string message) : base(message)
        {
        }

        public EventStoreStreamsBindingStreamDeletedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected EventStoreStreamsBindingStreamDeletedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
