using NHibernate.Type;

namespace IntelliFlo.Platform.Services.Workflow
{
    public class GenericEnumMapper<TEnum> : EnumStringType
    {
        public GenericEnumMapper()
            : base(typeof(TEnum))
        {
        }
    }
}