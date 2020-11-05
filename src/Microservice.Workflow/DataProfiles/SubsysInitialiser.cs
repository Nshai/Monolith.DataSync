using IntelliFlo.Platform.Database;
using IntelliFlo.Platform.Database.Impl.Initialisers;
using IntelliFlo.Platform.Database.Impl.Tasks;
using IntelliFlo.Platform.NHibernate;

namespace Microservice.Workflow.DataProfiles
{
    [DbProfile("subsys")]
    public class SubsysInitialiser : InitialiserBase
    {
        public SubsysInitialiser(IDatabaseInstance instance)
            : base(
                new DropDatabaseTask(instance),
                new CreateDatabaseTask(instance),
                new GrantReadAccessTask(instance),
                new MetadataLogPackageVersionLockingTask(instance,
                    new SynchroniseDatabaseTask(instance), new AddRefDataTask(instance),
                new CreatePersistenceDataStoreTask())
            )
        {
        }
    }
}
