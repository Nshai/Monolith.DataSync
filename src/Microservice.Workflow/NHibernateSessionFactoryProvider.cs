using System.Collections.Generic;
using IntelliFlo.Platform.NHibernate;

namespace Microservice.Workflow
{
    public class NHibernateSessionFactoryProvider : IntelliFlo.Platform.NHibernate.NHibernateSessionFactoryProvider
    {
        public NHibernateSessionFactoryProvider(INHibernateConfiguration configuration, IEnumerable<INHibernateInitializationAware> initializers) : base(configuration, initializers)
        {
            
        }
    }


}
