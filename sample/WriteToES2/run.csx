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

public static async Task<IActionResult> Run(HttpRequest req, IAsyncCollector<string> data, Microsoft.Extensions.Logging.ILogger log)
{
    string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

    await data.AddAsync(requestBody);
    log.LogInformation("C# HTTP trigger function processed a request.");

    return new OkObjectResult(requestBody);
}
