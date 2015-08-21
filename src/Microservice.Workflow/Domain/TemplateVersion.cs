using System;
using System.Collections.Generic;
using IntelliFlo.Platform;
using IntelliFlo.Platform.NHibernate;

namespace Microservice.Workflow.Domain
{
    public class TemplateVersion : EqualityAndHashCodeProvider<TemplateVersion, int>
    {
        private IList<TemplateRole> roles;
        private IList<TemplateTriggerSet> triggers;

        public TemplateVersion()
        {
            CreatedDate = DateTime.UtcNow;
            Definition = new WorkflowDefinition();
            Guid = GuidCombGenerator.Generate();
            roles = new List<TemplateRole>();
            triggers = new List<TemplateTriggerSet>();
        }

        public TemplateVersion(Template template) : this()
        {
            Template = template;
            TenantId = template.TenantId;
        }

        public virtual Guid Guid { get; set; }
        public virtual WorkflowStatus Status { get; set; }
        public virtual string Notes { get; set; }
        public virtual DateTime CreatedDate { get; set; }
        public virtual int OwnerUserId { get; set; }
        public virtual int TenantId { get; set; }
        public virtual Template Template { get; set; }
        public virtual long ConcurrencyId { get; set; }
        public virtual int? ApplicableToGroupId { get; set; }
        public virtual bool? IncludeSubGroups { get; set; }
        public virtual WorkflowDefinition Definition { get; set; }
        public virtual bool InUse { get; set; }

        public virtual IList<TemplateRole> Roles
        {
            get { return roles; }
            set { roles = value; }
        }

        public virtual IList<TemplateTriggerSet> Triggers
        {
            get { return triggers; }
            set { triggers = value; }
        }
    }
}