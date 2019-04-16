using EventStore.ClientAPI;
using Microsoft.Azure.WebJobs.Description;
using SiaConsulting.Azure.WebJobs.Extensions.EventStoreExtension.Streams.Bindings;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using Microsoft.Azure.WebJobs;
using System.Text;
using System.Linq;
using SiaConsulting.Azure.WebJobs.Extensions.EventStoreExtension.Streams.Exceptions;

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
            context.AddConverter<JObject, EventData>(input => input.ToObject<EventData>());
            context.AddConverter<JObject, IList<ResolvedEvent>>(input => input.ToObject<IList<ResolvedEvent>>());
            context.AddConverter<string, EventStoreData>(ConvertFromString);
            context.AddConverter<IList<ResolvedEvent>, JArray>(JArray.FromObject);
            context.AddConverter<IList<ResolvedEvent>, JObject>(JObject.FromObject);

            var eventStoreAttributeRule = context.AddBindingRule<EventStoreStreamsAttribute>();
            eventStoreAttributeRule.BindToInput<IList<ResolvedEvent>>(new EventStoreInputAsyncConverter(_logger));
            eventStoreAttributeRule.BindToCollector<EventStoreData>(config => new EventStoreDataAsyncCollector(config, _logger));
        }

        private EventStoreData ConvertFromString(string data)
        {
            var tmpData = JObject.Parse(data);
            var tmpEvents = tmpData["events"].ToObject<JArray>();
            var convertedEvents = tmpEvents.Select(ConvertEvent);
            return new EventStoreData(tmpData["streamName"]?.ToObject<string>(),tmpData["expectedVersion"]?.ToObject<long>() ?? ExpectedVersion.Any, convertedEvents);
        }

        private EventData ConvertEvent(JToken jToken)
        {
            var eventId = SafeParse(jToken["eventId"], JTokenType.Guid, Guid.NewGuid());
            var eventType = SafeParse<string>(jToken["type"], JTokenType.String, null);
            var eventIsJson = SafeParse(jToken["isJson"], JTokenType.Boolean, false);
            var eventData = ToBytes(jToken["data"]);
            var eventMetaData = ToBytes(jToken["metadata"]);
            return new EventData(eventId, eventType, eventIsJson, eventData, eventMetaData);
        }

        private byte[] ToBytes(JToken data)
        {
            if (data == null || data.Type == JTokenType.Null)
            {
                return null;
            }
            if(data.Type == JTokenType.String)
            {
                return Encoding.UTF8.GetBytes(data.ToObject<string>());
            }
            var jsonObject = data.ToObject<JObject>();
            return Encoding.UTF8.GetBytes(jsonObject.ToString());
        }

        private T SafeParse<T>(JToken token, JTokenType expectedJType, T defaultValue)
        {
            if (token == null || token.Type != expectedJType)
                return defaultValue;
            return token.ToObject<T>();
        }
    }
}
