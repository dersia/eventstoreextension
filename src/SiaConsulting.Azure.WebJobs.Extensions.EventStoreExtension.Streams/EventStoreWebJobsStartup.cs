using SiaConsulting.Azure.WebJobs.Extensions.EventStoreExtension.Streams;
using SiaConsulting.Azure.WebJobs.Extensions.EventStoreExtension.Streams.Config;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Azure.WebJobs;

[assembly: WebJobsStartup(typeof(EventStoreWebJobsStartup))]
namespace SiaConsulting.Azure.WebJobs.Extensions.EventStoreExtension.Streams
{
    public class EventStoreWebJobsStartup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder) 
            => builder.AddEventStoreStreams();
    }
}
