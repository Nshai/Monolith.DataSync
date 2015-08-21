using NHibernate.Type;

namespace Microservice.Workflow
{
    public class GenericEnumMapper<TEnum> : EnumStringType
    {
        public GenericEnumMapper()
            : base(typeof(TEnum))
        {
        }
    }
}