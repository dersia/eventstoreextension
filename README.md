This is a binding extension for Azure Functions

The extension supports output bindings and input bindings.
For output you can use an `IAsyncCollector<EventStoreData>` don't forget the using statement `using SiaConsulting.Azure.WebJobs.Extensions.EventStoreExtension.Streams.Bindings`
For input you can use `IList<ResolvedEvent>` don't forget the using statement `using EventStore.ClientAPI`

Important: the output binding uses transactions, this means, that the added events will only be flushed, if the function completes successfully

# Changelog
## Version 0.1.1 - bugfixes
* Fixed a bug where input binding fails, if the stream was deleted or does not exists 
## Version 0.1.0 - bugfixes + enhanced Types
* Fixed an error where input-bindings were not working with node-worker
* Fixed an error where output-bindings were not working with node-worker
* Added new Types for input and output bindings
	* Added support for `IAsyncCollector<EventData>`
	* Added support for `IAsyncCollector<string>`
	* Added support for `IAsyncCollector<byte[]>`
	* Added support for `IAsyncCollector<T>`
	* Added support for `IList<string>`
	* Added support for `IList<byte[]>`
	* Added support for `IList<T>`

## Version 0.0.6 - bugfixes + enhanced streamnames
* Fixed error "default streamname not working for writes": for out operations binding will now use streamname from `function.json` as default streamname, if no streamname is provided for `EventStoreData`.
* Added Prefix and Suffix for streamnames 
  * new config optional `StreamNamePrefix` can now be set to prefix a streamname for input and output bindings
  * new config optional `StreamNameSuffix` can now be set to suffix a streamname for input and output bindings
## Version 0.0.4 - first release
* Added binding extensions for EventStore input and output binding
* Added NugetPackage to NuGet.org

Sample function.json config for input bindig
```json5
{
	"type": "eventStoreStreams",
	"name": "eventStore",
	"direction": "in",
	"ConnectionStringSetting": "EventStoreConnection",
	//default for output 0.0.6
	"StreamName": "CobolIstDoofStream",
	//optional for prefixing streamnames 0.0.6
	"StreamNamePrefix": "Prefix-",
	//optional for suffixing streamnames 0.0.6
	"StreamNameSuffix": "-Suffix",
	"StreamOffset": -1,
	"ReadSize": -1,
	"ResolveLinkTos": false,
	"StreamReadDirection": "Forward"
}
```

Configname|Required|Default Value|Description
----------|--------|-------------|-----------
ConnectionStringSetting|Yes|N/A|Name of the appsetting that holds the connection string to the EventStore i.e. `tcp://admin:password@myeventstore:1113`
StreamName|Yes|N/A|Name fo the Stream to read from
StreamNamePrefix|No|`string.Empty`|Set a Prefix for streamnames. The Prefix is added to every streamname, not just the default streamname in case of output-binding and the use of `EventStoreData`
StreamNameSuffix|No|`string.Empty`|Set a Prefix for streamnames. The Suffix is added to every streamname, not just the default streamname in case of output-binding and the use of `EventStoreData`
StreamOffset|No|-1|Offset of the Stream reading from. -1 means from beginning
ReadSize|No|-1|Number of events to read. -1 means all events starting at offset
ResolveLinkTos|No|false|Resolve EventStore Links
StreamReadDirection|No|Forward|Direction of read. Available values are `Forward` and `Backward`. When `StreamReadDirection` is set to `Forward`, `StreamOffset` is set to `-1` and `ReadSize` is set to `-1` then you get all events from start to end of the stream. When `StreamReadDirection` is set to `Backward`, `StreamOffset` is set to `-1` and `ReadSize` is set to `-1` then you'll get all events from the end of the stream to the beginning.

Sample function.json config for output binding
```json
{
	"name": "data",
	"type": "eventStoreStreams",
	"direction": "out",
	"ConnectionStringSetting": "EventStoreConnection",
	"StreamName": "testStream"
}
```
Configname|Required|Default Value|Description
----------|--------|-------------|-----------
ConnectionStringSetting|Yes|N/A|Name of the appsetting that holds the connection string to the EventStore i.e. `tcp://admin:password@myeventstore:1113`
StreamName|Yes|N/A|Name fo the Stream to read from

Version `0.0.6` of this extension is available on nuget.

How to install

1. Create an Function App in the portal
2. Create a new Function within your function app
3. Get your functions url and you masterkey
4. use Postman or Curl to post the following to the extensions endpoint of your function app. If you functions url is `https://mycoolfunctionapp.azurewebsites.net/api/HttpTrigger1?code=ABC` then your extensions endpoint is `https://mycoolfunctionapp.azurewebsites.net/admin/host/extensions?code=ABC`
	```json
	{
		"Id":"SiaConsulting.Azure.WebJobs.Extensions.EventStoreExtension.Streams",
		"Version": "0.0.6"
	}
	```
5. check with the returned jobid, if the job to be completed / the extension is installed `https://mycoolfunctionapp.azurewebsites.net/admin/host/extensions/jobs/<JOBID>?code=ABC`
6. setup your function.json with all the needed parameters
7. start using the funtion

How to unsintall

There is a problem with uninstalling extensions right now, so the easiest way is to delete the functions app create a new one.
If you still want to uninstall the extension, this is how to do it

1. Stop the function app
2. Use Azure-Portal or Azure Storage Explorer to connect to the storage account file shares of your function app
3. Delete `EventStore.ClientAPI.NetCore.dll` and `SiaConsulting.Azure.WebJobs.Extensions.EventStoreExtension.Streams.dll` from `site/wwwroot/bin`
4. Edit `extensions.json` in `site/wwwroot/bin` and remove the `SiaConsulting.AzureWebJobs.Extensions.EventStoreExtension.Streams`-extension from the array
5. Edit `extensions.deps.json` in `site/wwwroot/bin` and remove any occurance of `EventStore.ClientAPI.NetCore` and `SiaConsulting.Azure.WebJobs.Extensions.EventStoreExtension.Streams`
6. Edit `extensions.csproj` in `site/wwwroot` and remove the `PackageReference` for `SiaConsulting.Azure.WebJobs.Extensions.EventStoreExtension.Streams`
7. Start your function app