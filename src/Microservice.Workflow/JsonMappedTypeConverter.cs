using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microservice.Workflow
{
    public class JsonMappedTypeConverter : JsonConverter
    {
        public IDictionary<string, Type> KnownTypes { get; set; }

        public JsonMappedTypeConverter() { }
        public JsonMappedTypeConverter(IDictionary<string, Type> knownTypes)
        {
            KnownTypes = knownTypes;
        }

        protected object Create(Type objectType, JObject jObject)
        {
            if (jObject["$key"] != null)
            {
                var keyName = jObject["$key"].ToString();
                return Activator.CreateInstance(KnownTypes[keyName]);
            }

            throw new InvalidOperationException("No supported key");
        }

        public override bool CanConvert(Type objectType)
        {
            if (KnownTypes == null)
                return false;
            return (objectType.IsInterface || objectType.IsAbstract) && KnownTypes.Values.Any(objectType.IsAssignableFrom);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            // Load JObject from stream
            var jObject = JObject.Load(reader);
            // Create target object based on JObject
            var target = Create(objectType, jObject);
            // Populate the object properties
            serializer.Populate(jObject.CreateReader(), target);

            return target;
        }

        public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            var jObject = JObject.FromObject(value);

            var key = KnownTypes.Single(v => v.Value == value.GetType()).Key;

            jObject.Add(new JProperty("$key", key));

            var json = jObject.ToString(Formatting.None);

            writer.WriteRawValue(json);
        }
    }
}