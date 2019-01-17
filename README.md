This is a binding extension for Azure Functions

The extension supports output bindings and input bindings.
For output you can use an `IAsyncCollector<EventStoreData>` don't forget the using statement `using SiaConsulting.Azure.WebJobs.Extensions.EventStoreExtension.Streams.Bindings`
For input you can use `IList<ResolvedEvent>` don't forget the using statement `using EventStore.ClientAPI`

Important: the output binding uses transactions, this means, that the added events will only be flushed, if the function completes successfully


How to install

1. Create an Function App in the portal
2. Create a new Function within your function app
3. Get a oauth token for the portal
4. Use Postman or Curl to post the following to the extensions endpoint of your function app, i.e. if you function app is called "mycoolfunctionapp", your url is "https://mycoolfunctionapp.azure.com/extension"
5. check with the returned jobid, if the job to be completed / the extension is installed
6. setup your function.json with all the needed parameters
7. start using the funtion

