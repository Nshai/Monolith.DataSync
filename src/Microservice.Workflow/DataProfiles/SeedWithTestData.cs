using System.Data;
using System.Linq;
using IntelliFlo.Platform.Database;
using IntelliFlo.Platform.NHibernate;
using Microservice.Workflow.Domain;
using NHibernate.Linq;

namespace Microservice.Workflow.DataProfiles
{
    public class SeedWithTestData : TaskBase<SeedWithTestData>
    {
        private readonly IReadWriteSessionFactoryProvider provider;
        private const int TenantId = 10155;
        private const int UserId = 28000;

        public SeedWithTestData(IReadWriteSessionFactoryProvider provider)
        {
            this.provider = provider;
        }

        public override object Execute(IDatabaseSettings settings)
        {
            using (var session = provider.SessionFactory.OpenSession())
            using (var tx = session.BeginTransaction(IsolationLevel.ReadUncommitted))
            {
                var templateCategory = session.Query<TemplateCategory>().SingleOrDefault(t => t.TenantId == TenantId && t.Name == "Test");
                if (templateCategory == null)
                {
                    templateCategory = new TemplateCategory("Test", TenantId);
                    session.Save(templateCategory);
                }

                if (!session.Query<Template>().Any(b => b.TenantId == TenantId))
                {
                    session.Save(new Template("Test", TenantId, templateCategory, WorkflowRelatedTo.Client, UserId));
                }

                tx.Commit();
                return true;
            }
        }

        public override void Dispose() { }
    }
}