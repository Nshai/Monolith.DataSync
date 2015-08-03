using System.Linq;

namespace IntelliFlo.Platform.Services.Workflow
{
    public class JsonMappedToTypeNameTypeListConverter<T, TDefault> : JsonMappedListConverter<T, TDefault>
    {
        public JsonMappedToTypeNameTypeListConverter() : base(typeof (T).Assembly.GetTypes().Where(typeof (T).IsAssignableFrom).ToDictionary(t => t.Name, t => t)) {}
    }
}