using Microsoft.Azure.WebJobs.Host.Config;
using Newtonsoft.Json.Linq;
using SiaConsulting.Azure.WebJobs.Extensions.EventStoreExtension.Streams.Bindings;
using EventStore.ClientAPI;
using Newtonsoft.Json;
using System.Dynamic;
using Microsoft.Azure.WebJobs.Host.Bindings;
using static SiaConsulting.Azure.WebJobs.Extensions.EventStoreExtension.Converter.CommonConverter;
using System.Collections.Generic;
using System.Linq;

namespace SiaConsulting.Azure.WebJobs.Extensions.EventStoreExtension.Converter
{
    public static class JsonStringConverter
    {
        public static ExtensionConfigContext AddAllConverters(this ExtensionConfigContext context, Microsoft.Extensions.Logging.ILogger logger)
        {
            return context
                .AddJsonConverter(logger)
                .AddEventDataConverter(logger)
                .AddJObjectConverter(logger)
                .AddDynamicConverter(logger)
                .AddExpandoObjectConverter(logger)
                .AddResolvedEventListConverter(logger)
                .AddResolvedEventConverter(logger)
                .AddResolvedEventToStringConverter(logger)
                .AddResolvedEventListToStringConverter(logger)
                .AddResolvedEventToByteArrayConverter(logger)
                .AddResolvedEventListToByteArrayListConverter(logger)
                .AddOpenConverter<IList<OpenType>, IList<EventStoreData>>(typeof(ToOpenTypeListConverter<>), logger)
                .AddOpenConverter<OpenType, EventStoreData>(typeof(ToOpenTypeConverter<>), logger)
                .AddOpenConverter<IList<ResolvedEvent>, IList<OpenType>>(typeof(FromOpenTypeConverter<>), logger);
        }

        public static ExtensionConfigContext AddJsonConverter(this ExtensionConfigContext context, Microsoft.Extensions.Logging.ILogger logger) 
            => context.AddConverter<string, EventStoreData>(payload =>
                                                            {
                                                                if (IsValidJson(payload, logger))
                                                                {
                                                                    var jt = JToken.Parse(payload);
                                                                    if (IsJsonOfTypeEventStoreData(jt, logger))
                                                                    {
                                                                        return ParseEventStoreData(jt);
                                                                    }
                                                                    if (IsJsonOfTypeEventData(jt, logger))
                                                                    {
                                                                        return BuildFromEventData(ParseEventData(jt));
                                                                    }
                                                                }
                                                                return BuildFromEventData(BuildFromPayload(payload, logger));
                                                            });

        public static ExtensionConfigContext AddEventDataConverter(this ExtensionConfigContext context, Microsoft.Extensions.Logging.ILogger logger) 
            => context.AddConverter<EventData, EventStoreData>(BuildFromEventData);

        public static ExtensionConfigContext AddJObjectConverter(this ExtensionConfigContext context, Microsoft.Extensions.Logging.ILogger logger) 
            => context.AddConverter<JObject, EventData>(payload => BuildFromPayload(payload.ToString(), logger));

        public static ExtensionConfigContext AddDynamicConverter(this ExtensionConfigContext context, Microsoft.Extensions.Logging.ILogger logger) 
            => context.AddConverter<dynamic, EventData>(payload => BuildFromPayload(JsonConvert.SerializeObject(payload), logger));

        public static ExtensionConfigContext AddExpandoObjectConverter(this ExtensionConfigContext context, Microsoft.Extensions.Logging.ILogger logger) 
            => context.AddConverter<ExpandoObject, EventData>(payload => BuildFromPayload(JsonConvert.SerializeObject(payload), logger));

        public static ExtensionConfigContext AddResolvedEventListConverter(this ExtensionConfigContext context, Microsoft.Extensions.Logging.ILogger logger)
            => context.AddConverter<IList<ResolvedEvent>, JArray>(re => JArray.FromObject(re.Select(e => ToJObject(e, logger))));

        public static ExtensionConfigContext AddResolvedEventConverter(this ExtensionConfigContext context, Microsoft.Extensions.Logging.ILogger logger)
            => context.AddConverter<ResolvedEvent, JObject>(re => ToJObject(re, logger));

        public static ExtensionConfigContext AddResolvedEventToStringConverter(this ExtensionConfigContext context, Microsoft.Extensions.Logging.ILogger logger)
            => context.AddConverter<ResolvedEvent, string>(re => ToJObject(re, logger).ToString());

        public static ExtensionConfigContext AddResolvedEventListToStringConverter(this ExtensionConfigContext context, Microsoft.Extensions.Logging.ILogger logger)
            => context.AddConverter<IList<ResolvedEvent>, string>(re => JArray.FromObject(re.Select(e => ToJObject(e, logger))).ToString());

        public static ExtensionConfigContext AddResolvedEventToByteArrayConverter(this ExtensionConfigContext context, Microsoft.Extensions.Logging.ILogger logger)
            => context.AddConverter<ResolvedEvent, byte[]>(re => re.Event?.Data);

        public static ExtensionConfigContext AddResolvedEventListToByteArrayListConverter(this ExtensionConfigContext context, Microsoft.Extensions.Logging.ILogger logger)
            => context.AddConverter<IList<ResolvedEvent>, IList<byte[]>>(re => re.Where(e => e.Event?.Data != null).Select(e => e.Event.Data).ToList());
    }
}
