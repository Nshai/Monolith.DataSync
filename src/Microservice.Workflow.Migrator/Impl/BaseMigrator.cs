using System;
using System.Threading.Tasks;
using Microservice.Workflow.Migrator.Collaborators;
using NLog;

namespace Microservice.Workflow.Migrator.Impl
{
    public abstract class BaseMigrator<T> : IMigrator where T : IRepresentation
    {
        private readonly string name;
        protected readonly MigrateConfiguration configuration;
        private const int PageCount = 10;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        protected BaseMigrator(string name, MigrateConfiguration configuration)
        {
            this.name = name;
            this.configuration = configuration;
        }

        public async Task Execute(Action<int> intialise, Action<string> tick)
        {
            using (var client = new Client())
            {
                await client.InitialiseToken();

                var initialised = false;
                var pageIndex = 0;
                var count = 0;
                var index = 0;
                var skippedCount = 0;
                var failedCount = 0;

                while (true)
                {
                    var batchNumber = pageIndex + 1;
                    var instanceCollection = await GetItems(client, pageIndex);

                    if (!initialised)
                    {
                        count = instanceCollection.Count;
                        if (count <= 0)
                        {
                            logger.Info("No {0}s found", name.ToLowerInvariant());
                            return;
                        }

                        intialise(count);
                        initialised = true;
                    }

                    foreach (var instance in instanceCollection.Items)
                    {
                        logger.Info("Migrating {0} {1}", name.ToLowerInvariant(), GetId(instance));
                        MigrationStatus status;
                        try
                        {
                            var response = await MigrateItem(client, GetId(instance));
                            status = (MigrationStatus) Enum.Parse(typeof (MigrationStatus), response.Status);
                            logger.Info("{0} {1} {2}", name, GetId(instance), response.Status.ToLowerInvariant());
                        }
                        catch (Exception ex)
                        {
                            status = MigrationStatus.Failed;
                            logger.Error(ex, "Skipping {0} {1} due to error", name.ToLowerInvariant(), GetId(instance));
                        }

                        if (status == MigrationStatus.Skipped)
                            skippedCount++;

                        if (status == MigrationStatus.Failed)
                            failedCount++;

                        index++;
                        tick(string.Format("Migrating {0}... {1} of {2} ({3} skipped, {4} failed)", name.ToLowerInvariant(), index, count, skippedCount, failedCount));
                    }

                    if (instanceCollection.Count < (batchNumber * PageCount))
                    {
                        break;
                    }

                    pageIndex++;
                }
            }
        }

        public abstract Task<PagedCollection<T>> GetItems(Client client, int page);
        public abstract Task<MigrationResponse> MigrateItem(Client client, string id);

        public abstract string GetId(T item);
    }
}