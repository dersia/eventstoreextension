using EventStore.ClientAPI;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SiaConsulting.Azure.WebJobs.Extensions.EventStoreExtension.Streams.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiaConsulting.Azure.WebJobs.Extensions.EventStoreExtension.Converter
{
    public static class CommonConverter
    {
        public static EventData BuildFromPayload(string payload, Microsoft.Extensions.Logging.ILogger logger)
        {
            var isJson = false;
            if (IsValidJson(payload, logger))
            {
                isJson = true;
            }
            return new EventData(Guid.NewGuid(), "Unkown", isJson, ToBytes(payload, isJson), null);
        }

        public static EventStoreData ParseEventStoreData(JToken @event)
        {
            var streamName = SafeParse(@event["streamName"], JTokenType.String, SafeParse<string>(@event["StreamName"], JTokenType.String, null));
            var expectedVersion = SafeParse(@event["expectedVersion"], JTokenType.Integer, SafeParse(@event["ExpectedVersion"], JTokenType.Integer, ExpectedVersion.Any));
            var jArrayEvents = SafeParse(@event["events"], JTokenType.Array, SafeParse(@event["Events"], JTokenType.Array, new JArray()));
            var @events = jArrayEvents.Select(ParseEventData);
            return new EventStoreData(streamName, expectedVersion, @events);
        }

        public static EventData ParseEventData(JToken @event)
        {
            var eventId = SafeParse(@event["eventId"], JTokenType.Guid, SafeParse(@event["EventId"], JTokenType.Guid, Guid.NewGuid()));
            var eventType = SafeStringParse(@event["type"], JTokenType.String, SafeStringParse(@event["Type"], JTokenType.String, "Unknown"));
            var eventIsJson = SafeParse(@event["isJson"], JTokenType.Boolean, SafeParse(@event["IsJson"], JTokenType.Boolean, false));
            var eventData = ToBytes(@event["data"], ToBytes(@event["Data"], null));
            var eventMetaData = ToBytes(@event["metadata"], ToBytes(@event["Metadata"], null));
            return new EventData(eventId, eventType, eventIsJson, eventData, eventMetaData);
        }

        private static T SafeParse<T>(JToken token, JTokenType expectedJType, T defaultValue)
        {
            if (token == null || token.Type != expectedJType)
                return defaultValue;
            return token.ToObject<T>();
        }

        private static string SafeStringParse(JToken token, JTokenType expectedJType, string defaultValue)
        {
            if (token == null || token.Type != expectedJType)
                return defaultValue;
            var result = token.ToObject<string>();
            if(string.IsNullOrWhiteSpace(result))
            {
                result = defaultValue;
            }
            return result;
        }

        private static byte[] ToBytes(JToken data, byte[] defaultValue)
        {
            if (data == null || data.Type == JTokenType.Null)
            {
                return defaultValue;
            }
            return ToBytes(data.ToString(), data.Type == JTokenType.String);
        }

        private static byte[] ToBytes(string payload, bool isJson)
        {
            if (isJson)
            {
                var jsonObject = JObject.Parse(payload);
                return Encoding.UTF8.GetBytes(jsonObject.ToString());
            }
            return Encoding.UTF8.GetBytes(payload);
        }

        public static EventStoreData BuildFromEventData(EventData eventData)
            => new EventStoreData { Events = new List<EventData> { eventData } };

        public static bool IsJsonOfTypeEventStoreData(JToken json, Microsoft.Extensions.Logging.ILogger logger)
        {
            try
            {
                if ((json["events"] != null && json["events"].Type == JTokenType.Array)
                    || (json["Events"] != null && json["Events"].Type == JTokenType.Array))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.LogTrace(ex, ex.Message);
                logger.LogTrace($"Json seams not to be of Type {nameof(EventStoreData)}.");
            }
            return false;
        }

        public static bool IsJsonOfTypeEventData(JToken json, Microsoft.Extensions.Logging.ILogger logger)
        {
            try
            {
                if ((json["data"] != null && json["type"] != null)
                    || (json["Data"] != null && json["Type"] != null))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.LogTrace(ex, ex.Message);
                logger.LogTrace($"Json seams not to be of Type {nameof(EventStoreData)}.");
            }
            return false;
        }

        public static bool IsValidJson(string strInput, Microsoft.Extensions.Logging.ILogger logger)
        {
            strInput = strInput.Trim();
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
                (strInput.StartsWith("[") && strInput.EndsWith("]"))) //For array
            {
                try
                {
                    var obj = JToken.Parse(strInput);
                    return true;
                }
                catch (JsonReaderException jex)
                {
                    //Exception in parsing json
                    logger.LogTrace(jex, jex.Message);
                    return false;
                }
                catch (Exception ex) //some other exception
                {
                    logger.LogTrace(ex, ex.ToString());
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public static JObject ToJObject(ResolvedEvent rEvent, Microsoft.Extensions.Logging.ILogger logger)
        {
            var deserializedEvent = JObject.FromObject(rEvent);
            if (rEvent.Event.Data != null)
            {
                var deserializedData = Encoding.UTF8.GetString(rEvent.Event.Data);
                if(IsValidJson(deserializedData, logger))
                {
                    deserializedEvent["Event"]["Data"] = JObject.Parse(deserializedData);
                }
                else
                {
                    deserializedEvent["Event"]["Data"] = deserializedData;
                }
            }
            if (rEvent.Event.Metadata != null)
            {
                var deserializedData = Encoding.UTF8.GetString(rEvent.Event.Metadata);
                if (IsValidJson(deserializedData, logger))
                {
                    deserializedEvent["Event"]["Metadata"] = JObject.Parse(deserializedData);
                }
                else
                {
                    deserializedEvent["Event"]["Metadata"] = deserializedData;
                }
            }
            if (rEvent.OriginalEvent.Data != null)
            {
                var deserializedData = Encoding.UTF8.GetString(rEvent.OriginalEvent.Data);
                if (IsValidJson(deserializedData, logger))
                {
                    deserializedEvent["OriginalEvent"]["Data"] = JObject.Parse(deserializedData);
                }
                else
                {
                    deserializedEvent["OriginalEvent"]["Data"] = deserializedData;
                }
            }
            if (rEvent.OriginalEvent.Metadata != null)
            {
                var deserializedData = Encoding.UTF8.GetString(rEvent.OriginalEvent.Metadata);
                if (IsValidJson(deserializedData, logger))
                {
                    deserializedEvent["OriginalEvent"]["Metadata"] = JObject.Parse(deserializedData);
                }
                else
                {
                    deserializedEvent["OriginalEvent"]["Metadata"] = deserializedData;
                }
            }
            return deserializedEvent;
        }
    }
}
