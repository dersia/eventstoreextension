This is a binding extension for Azure Functions

The extension supports output bindings and input bindings.
For output you can use an `IAsyncCollector<EventStoreData>` don't forget the using statement `using SiaConsulting.Azure.WebJobs.Extensions.EventStoreExtension.Streams.Bindings`
For input you can use `IList<ResolvedEvent>` don't forget the using statement `using EventStore.ClientAPI`

Important: the output binding uses transactions, this means, that the added events will only be flushed, if the function completes successfully


How to install

1. Create an Function App in the portal
2. Create a new Function within your function app
3. Get your functions url and you masterkey
4. use Postman or Curl to post the following to the extensions endpoint of your function app. If you functions url is `https://mycoolfunctionapp.azurewebsites.net/api/HttpTrigger1?code=ABC` then your extensions endpoint is `https://mycoolfunctionapp.azurewebsites.net/admin/host/extensions?code=ABC`
	```json
	{
		"Id":"SiaConsulting.Azure.WebJobs.Extensions.EventStoreExtension.Streams",
		"Version": "0.0.3-alpha"
	}
	```
5. check with the returned jobid, if the job to be completed / the extension is installed `https://mycoolfunctionapp.azurewebsites.net/admin/host/extensions/jobs/<JOBID>?code=ABC`
6. setup your function.json with all the needed parameters
7. start using the funtion

