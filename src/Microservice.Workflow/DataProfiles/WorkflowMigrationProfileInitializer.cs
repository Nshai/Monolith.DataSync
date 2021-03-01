using IntelliFlo.Platform.Database;
using IntelliFlo.Platform.Database.Impl.Initialisers;
using IntelliFlo.Platform.Database.Impl.Tasks;

namespace Microservice.Workflow.DataProfiles
{
    [DbProfile("migrate")]
    public class WorkflowMigrationProfileInitializer : InitialiserBase
    {
        public WorkflowMigrationProfileInitializer(IDatabaseInstance instance)
             : base(new CreateDatabaseTask(instance),
                   new MetadataLogPackageVersionLockingTask(instance, new SynchroniseDatabaseTask(instance), new AddRefDataTask(instance)),
                   new CreatePersistenceDataStoreTask(dropDatabase: false, useSafetyCheck: true),
                   new UpgradePersistenceDataStoreSchemaTask())
        {
        }
    }
}
