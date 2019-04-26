#r "Newtonsoft.Json"
#r "../bin/SiaConsulting.Azure.WebJobs.Extensions.EventStoreExtension.Streams.dll"
#r "../bin/EventStore.ClientAPI.NetCore.dll"

using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using SiaConsulting.Azure.WebJobs.Extensions.EventStoreExtension.Streams.Bindings;
using EventStore.ClientAPI;
using System;

public static async Task<IActionResult> Run(HttpRequest req, IAsyncCollector<EventData> data, Microsoft.Extensions.Logging.ILogger log)
{
    string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
    var @event = new EventData(Guid.NewGuid(), "TESTType", true, System.Text.Encoding.UTF8.GetBytes(requestBody), null);

    await data.AddAsync(@event);
    log.LogInformation("C# HTTP trigger function processed a request.");

    return new OkObjectResult(data);
}
