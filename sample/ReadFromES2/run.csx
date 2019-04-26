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

public static async Task<IActionResult> Run(HttpRequest req, IList<ResolvedEvent> eventStore, Microsoft.Extensions.Logging.ILogger log)
{
    return new OkObjectResult(string.Join("\r\n\r\n", eventStore.ToArray()));
}
