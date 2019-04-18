using EventStore.ClientAPI;
using Microsoft.Azure.WebJobs.Description;
using SiaConsulting.Azure.WebJobs.Extensions.EventStoreExtension.Streams.Bindings;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using Microsoft.Azure.WebJobs;
using SiaConsulting.Azure.WebJobs.Extensions.EventStoreExtension.Converter;

namespace SiaConsulting.Azure.WebJobs.Extensions.EventStoreExtension.Streams.Config
{
    [Extension("EventStoreStreams")]
    public class EventStoreStreamsConfigProvider : IExtensionConfigProvider
    {
        private readonly INameResolver _nameResolver;
        private readonly Microsoft.Extensions.Logging.ILogger _logger;

        public EventStoreStreamsConfigProvider(INameResolver nameResolver, ILoggerFactory loggerFactory)
        {
            _nameResolver = nameResolver;
            _logger = loggerFactory.CreateLogger("EventStoreStreams");
        }

        public void Initialize(ExtensionConfigContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            context.AddConverter<string, JObject>(JObject.FromObject);
            context.AddConverter<JObject, ResolvedEvent?>(input => input.ToObject<ResolvedEvent?>());
            context.AddConverter<JObject, IList<ResolvedEvent>>(input => input.ToObject<IList<ResolvedEvent>>());
            context.AddAllConverters(_logger);

            var eventStoreAttributeRule = context.AddBindingRule<EventStoreStreamsAttribute>();
            eventStoreAttributeRule.BindToInput<IList<ResolvedEvent>>(new EventStoreInputAsyncConverter(_logger));
            eventStoreAttributeRule.BindToCollector<EventStoreData>(config => new EventStoreDataAsyncCollector(config, _logger));
        }
    }
}
