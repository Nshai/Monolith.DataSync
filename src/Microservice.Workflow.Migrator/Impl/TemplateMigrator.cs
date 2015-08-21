using System.Globalization;
using System.Threading.Tasks;
using Microservice.Workflow.Migrator.Collaborators;

namespace Microservice.Workflow.Migrator.Impl
{
    public class TemplateMigrator : BaseMigrator<TemplateDocument>
    {
        public TemplateMigrator(MigrateConfiguration configuration) : base("Template", configuration) {}

        public override async Task<PagedCollection<TemplateDocument>> GetItems(Client client, int page)
        {
            var uri = string.Format("v1/migrate/templates?$skip={0}&$top={1}", page * configuration.BatchSize, configuration.BatchSize);
            if (configuration.TemplateId.HasValue)
                uri += string.Format("&$filter=Id eq {0}", configuration.TemplateId);
            return await client.Get<PagedCollection<TemplateDocument>>(uri);
        }

        public override async Task<MigrationResponse> MigrateItem(Client client, string id)
        {
            var uri = string.Format("v1/migrate/templates/{0}", id);
            return await client.Post<MigrationResponse>(uri);
        }

        public override string GetId(TemplateDocument item)
        {
            return item.Id.ToString(CultureInfo.InvariantCulture);
        }
    }
}
