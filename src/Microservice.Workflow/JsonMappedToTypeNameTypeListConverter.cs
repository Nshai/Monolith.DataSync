using System.Linq;

namespace Microservice.Workflow
{
    public class JsonMappedToTypeNameTypeListConverter<T, TDefault> : JsonMappedListConverter<T, TDefault>
    {
        public JsonMappedToTypeNameTypeListConverter() : base(typeof (T).Assembly.GetTypes().Where(typeof (T).IsAssignableFrom).ToDictionary(t => t.Name, t => t)) {}
    }
}