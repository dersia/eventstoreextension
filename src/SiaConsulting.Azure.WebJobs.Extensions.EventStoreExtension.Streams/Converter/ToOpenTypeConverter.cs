using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;
using SiaConsulting.Azure.WebJobs.Extensions.EventStoreExtension.Streams.Bindings;
using System.Collections.Generic;
using System.Linq;
using static SiaConsulting.Azure.WebJobs.Extensions.EventStoreExtension.Converter.CommonConverter;

namespace SiaConsulting.Azure.WebJobs.Extensions.EventStoreExtension.Converter
{
    public class ToOpenTypeConverter<T> : IConverter<T, EventStoreData>
    {
        private readonly Microsoft.Extensions.Logging.ILogger _logger;

        public ToOpenTypeConverter(Microsoft.Extensions.Logging.ILogger logger)
        {
            _logger = logger;
        }

        public EventStoreData Convert(T input) 
            => BuildFromEventData(BuildFromPayload(JsonConvert.SerializeObject(input), _logger));
    }

    public class ToOpenTypeListConverter<T> : IConverter<IList<T>, IList<EventStoreData>>
    {
        private readonly Microsoft.Extensions.Logging.ILogger _logger;

        public ToOpenTypeListConverter(Microsoft.Extensions.Logging.ILogger logger)
        {
            _logger = logger;
        }

        public IList<EventStoreData> Convert(IList<T> input)
            => input.Select(i => BuildFromEventData(BuildFromPayload(JsonConvert.SerializeObject(i), _logger))).ToList();
    }
}
