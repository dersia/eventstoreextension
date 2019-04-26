using System;
using System.Runtime.Serialization;

namespace SiaConsulting.Azure.WebJobs.Extensions.EventStoreExtension.Streams.Exceptions
{
    public class EventStoreStreamsBindingStreamNotFoundException : EventStoreStreamsBindingException
    {
        public EventStoreStreamsBindingStreamNotFoundException()
        {
        }

        public EventStoreStreamsBindingStreamNotFoundException(string message) : base(message)
        {
        }

        public EventStoreStreamsBindingStreamNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected EventStoreStreamsBindingStreamNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
