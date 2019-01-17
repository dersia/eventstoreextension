using Microsoft.Azure.WebJobs;
using System;

namespace SiaConsulting.Azure.WebJobs.Extensions.EventStoreExtension.Streams.Config
{
    public static class EventStoreStreamsWebJobsBuilderExtensions
    {
        public static IWebJobsBuilder AddEventStoreStreams(this IWebJobsBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.AddExtension<EventStoreStreamsConfigProvider>();

            return builder;
        }
    }
}
