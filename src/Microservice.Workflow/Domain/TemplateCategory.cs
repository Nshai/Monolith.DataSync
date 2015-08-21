using IntelliFlo.Platform.NHibernate;

namespace Microservice.Workflow.Domain
{
    public class TemplateCategory : EqualityAndHashCodeProvider<TemplateCategory, int>
    {
        protected TemplateCategory() {}
        public TemplateCategory(string name, int tenantId)
        {
            Name = name;
            TenantId = tenantId;
        }

        public virtual string Name { get; set; }
        public virtual bool IsArchived { get; set; }
        public virtual int TenantId { get; set; }
        public virtual long ConcurrencyId { get; set; }
    }
}