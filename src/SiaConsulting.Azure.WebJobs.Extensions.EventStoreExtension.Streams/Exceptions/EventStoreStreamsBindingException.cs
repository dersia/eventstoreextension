using System;
using System.Runtime.Serialization;

namespace SiaConsulting.Azure.WebJobs.Extensions.EventStoreExtension.Streams.Exceptions
{
    public class EventStoreStreamsBindingException : Exception
    {
        public EventStoreStreamsBindingException()
        {
        }

        public EventStoreStreamsBindingException(string message) : base(message)
        {
        }

        public EventStoreStreamsBindingException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected EventStoreStreamsBindingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
