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
        [AutoResolve]
        public string StreamName { get; set; }
        [AutoResolve]
        public string StreamNamePrefix { get; set; }
        [AutoResolve]
        public string StreamNameSuffix { get; set; }
        public long StreamOffset { get; set; } = 0;
        public int ReadSize { get; set; } = -1;
        public bool ResolveLinkTos { get; set; } = false;
        public StreamReadDirection StreamReadDirection { get; set; }
    }
}
