using System;
using System.Threading;
using IntelliFlo.Platform.NHibernate;
using IntelliFlo.Platform.Principal;

namespace Microservice.Workflow.Domain
{
    public class TemplateRegistration : EqualityAndHashCodeProvider<TemplateRegistration, int>
    {
        protected TemplateRegistration() {}
        public TemplateRegistration(string identifier, TemplateDefinition templateDefinition)
        {
            Identifier = identifier;
            Template = templateDefinition;

            TenantId = Thread.CurrentPrincipal.AsIFloPrincipal().TenantId;
        }

        public virtual string Identifier { get; set; }
        public virtual int TenantId { get; set; }
        public virtual int ConcurrencyId { get; set; }
        public virtual TemplateDefinition Template { get; set; }
    }
}
