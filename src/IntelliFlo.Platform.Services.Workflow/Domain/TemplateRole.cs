using IntelliFlo.Platform.NHibernate;

namespace IntelliFlo.Platform.Services.Workflow.Domain
{
    public class TemplateRole : EqualityAndHashCodeProvider<TemplateRole, int>
    {
        public virtual TemplateVersion TemplateVersion { get; set; }
        public virtual int TenantId { get; set; }
        public virtual int RoleId { get; set; }
        public virtual int ConcurrencyId { get; set; }
    }
}
