using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IntelliFlo.Platform.NHibernate;

namespace Microservice.Workflow
{
    [Obsolete("Will be removed once scheduler is introduced")]
    public interface IHostSessionFactoryProvider : ISessionFactoryProvider
    {
    }
}
