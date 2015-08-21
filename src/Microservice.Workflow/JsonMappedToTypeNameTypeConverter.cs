using System.Linq;

namespace Microservice.Workflow
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
