using Microsoft.Azure.WebJobs.Description;
using System;

namespace SiaConsulting.Azure.WebJobs.Extensions.EventStoreExtension.Streams
{
    [AttributeUsage(AttributeTargets.ReturnValue | AttributeTargets.Parameter)]
    [Binding]
    public class EventStoreStreamsAttribute : Attribute
    {
        [AppSetting]
        public string ConnectionStringSetting { get; set; }

        public string StreamName { get; set; }
        public long StreamOffset { get; set; }
        public int ReadSize { get; set; }
        public bool ResolveLinkTos { get; set; }
        public StreamReadDirection StreamReadDirection { get; set; }
    }
}
