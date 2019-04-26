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

public static async Task<IActionResult> Run(HttpRequest req, IAsyncCollector<CoolEvent> data, Microsoft.Extensions.Logging.ILogger log)
{
    string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

    await data.AddAsync(new CoolEvent { RequestPayload = requestBody });
    log.LogInformation("C# HTTP trigger function processed a request.");

    return new OkObjectResult(data);
}

public class CoolEvent 
{
    public string RequestPayload { get; set; }
}