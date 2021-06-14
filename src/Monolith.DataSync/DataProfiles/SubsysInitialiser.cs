using IntelliFlo.Platform.Database;
using IntelliFlo.Platform.Database.Impl.Initialisers;
using IntelliFlo.Platform.Database.Impl.Tasks;

namespace Microservice.DataSync.DataProfiles
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
                new CreatePersistenceDataStoreTask(dropDatabase: true, useSafetyCheck: false))
            )
        {
        }
    }
}
