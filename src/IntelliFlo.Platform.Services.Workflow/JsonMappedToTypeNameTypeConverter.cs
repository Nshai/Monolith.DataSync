using System.Linq;

namespace IntelliFlo.Platform.Services.Workflow
{
    public class JsonMappedToTypeNameTypeConverter<T1> : JsonMappedTypeConverter
    {
        public JsonMappedToTypeNameTypeConverter()
        {
            var type = typeof(T1);
            KnownTypes = type.Assembly.GetTypes().Where(type.IsAssignableFrom).ToDictionary(t => t.Name, t => t);
        }
    }
}
