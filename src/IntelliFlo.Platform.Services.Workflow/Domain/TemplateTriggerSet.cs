using System;
using System.Linq;
using IntelliFlo.Platform.NHibernate;

namespace IntelliFlo.Platform.Services.Workflow.Domain
{
    [Serializable]
    public class TemplateTriggerSet : EqualityAndHashCodeProvider<TemplateTriggerSet, int>
    {
        protected TemplateTriggerSet()
        {
            PropertyList = new TriggerPropertyList();
        }

        public TemplateTriggerSet(TemplateVersion templateVersion, int tenantId, TriggerType triggerType) : this()
        {
            TemplateVersion = templateVersion;
            TenantId = tenantId;
            TriggerType = triggerType;
        }

        public virtual TriggerType TriggerType { get; set; }
        public virtual TemplateVersion TemplateVersion { get; set; }
        public virtual int TenantId { get; set; }
        public virtual int EventSubscriptionId { get; set; }
        public virtual TriggerPropertyList PropertyList { get; set; }
        public virtual int ConcurrencyId { get; set; }

        public virtual BaseTrigger Trigger
        {
            get
            {
                var trigger = TemplateTriggerFactory.Create(TriggerType);
                if (PropertyList != null && PropertyList.Properties.Any())
                    trigger.Deserialize(PropertyList.Properties);
                return trigger;
            }
            set
            {
                PropertyList = new TriggerPropertyList(value.Serialize());
            }
        }
    }
}