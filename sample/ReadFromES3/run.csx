#r "Newtonsoft.Json"
#r "../bin/SiaConsulting.Azure.WebJobs.Extensions.EventStoreExtension.Streams.dll"
#r "../bin/EventStore.ClientAPI.NetCore.dll"

using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using EventStore.ClientAPI;
using System;
using System.Linq;

public static async Task<IActionResult> Run(HttpRequest req, IList<byte[]> eventStore, Microsoft.Extensions.Logging.ILogger log)
{
    var events = eventStore.Select(e => System.Text.Encoding.UTF8.GetString(e.Data)).ToArray();
    return new OkObjectResult(string.Join("\r\n\r\n", events));
}
