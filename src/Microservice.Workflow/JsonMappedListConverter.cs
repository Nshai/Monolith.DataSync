using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microservice.Workflow
{
    public class JsonMappedListConverter<T, TDefault> : JsonConverter
    {
        private IDictionary<string, Type> knownTypes;

        public IDictionary<string, Type> KnownTypes
        {
            get { return knownTypes; }
        }

        public JsonMappedListConverter() { }
        public JsonMappedListConverter(IDictionary<string, Type> knownTypes)
        {
            this.knownTypes = knownTypes;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var list = (IList<T>)value;

            writer.WriteStartArray();
            foreach (var item in list)
            {
                var jObject = JObject.FromObject(item);
                if (item.GetType() != typeof(TDefault))
                {
                    var key = KnownTypes.Single(v => v.Value == item.GetType()).Key;
                    jObject.Add(new JProperty("$key", key));
                }
                var json = jObject.ToString(Formatting.None);

                writer.WriteRawValue(json);
            }
            writer.WriteEndArray();
        }

        private Type GetObjectType(JObject jObject)
        {
            if (jObject["$key"] != null)
            {
                var keyName = jObject["$key"].ToString();
                string typeName = KnownTypes[keyName].FullName;
                return Type.GetType(typeName);
            }
            else
            {
                return typeof(TDefault);
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            IList<T> list = new List<T>();

            reader.Read();
            while (reader.TokenType != JsonToken.EndArray)
            {
                JObject jObject = JObject.Load(reader);

                list.Add((T)serializer.Deserialize(new JTokenReader(jObject), GetObjectType(jObject)));
                reader.Read();
            }

            return list;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(IList<T>) == objectType;
        }
    }
}
