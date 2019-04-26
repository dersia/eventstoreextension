using EventStore.ClientAPI;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiaConsulting.Azure.WebJobs.Extensions.EventStoreExtension.Converter
{
    public class FromOpenTypeConverter<T> : IConverter<IList<ResolvedEvent>, IList<T>>
    {
        private readonly Microsoft.Extensions.Logging.ILogger _logger;

        public FromOpenTypeConverter(Microsoft.Extensions.Logging.ILogger logger)
        {
            _logger = logger;
        }

        public IList<T> Convert(IList<ResolvedEvent> input) 
            => input
            .Where(re => re.Event?.Data != null)
            .Select(re => JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(re.Event.Data)))
            .ToList();
    }
}
