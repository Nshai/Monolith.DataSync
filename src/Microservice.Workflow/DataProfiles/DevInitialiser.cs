using IntelliFlo.Platform.Database;
using IntelliFlo.Platform.Database.Impl.Initialisers;
using IntelliFlo.Platform.Database.Impl.Tasks;
using IntelliFlo.Platform.NHibernate;

namespace Microservice.Workflow.DataProfiles
{
    [DbProfile("dev")]
    public class DevInitialiser : InitialiserBase
    {
        public DevInitialiser(IDatabaseInstance instance, IReadWriteSessionFactoryProvider provider)
            : base(new CreateDatabaseTask(instance),
                  new SynchroniseDatabaseTask(instance){ IncludeObjectTypes = "UserDefinedType,Assembly,Table,Function,View,StoredProcedure"},
                  new AddRefDataTask(instance),
                  new SeedWithTestData(provider))
        {
            
        }
    }
}
