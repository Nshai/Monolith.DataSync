using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Autofac;
using IntelliFlo.Platform.NHibernate;

namespace Microservice.Workflow.Modules
{
    [ExcludeFromCodeCoverage]
    public class NHibernateConfigurationModule : AutofacNHibernateBaseModule
    {
        private readonly string readConnectionStringName;
        private readonly string writeConnectionStringName;

        public NHibernateConfigurationModule(string readConnectionStringName, string writeConnectionStringName)
        {
            this.readConnectionStringName = readConnectionStringName;
            this.writeConnectionStringName = writeConnectionStringName;
        }

        protected override INHibernateConfiguration CreateReadWriteConfiguration(IComponentContext context)
        {
            return new ConfigureNHibernateForMsSql2005(writeConnectionStringName, Assembly.GetExecutingAssembly());
        }

        protected override INHibernateConfiguration CreateReadOnlyConfiguration(IComponentContext context)
        {
            return new ConfigureNHibernateForMsSql2005(readConnectionStringName, Assembly.GetExecutingAssembly());
        }
    }
}
