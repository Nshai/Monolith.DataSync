using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace IntelliFlo.Platform.Services.Workflow.Domain
{
    [Serializable]
    [XmlRoot("TriggerList")]
    public class TriggerPropertyList
    {
        public TriggerPropertyList()
        {
            Properties = new List<BaseTriggerProperty>();
        }

        public TriggerPropertyList(IEnumerable<BaseTriggerProperty> properties) : this()
        {
            Properties.AddRange(properties);
        }

        [XmlArray("Triggers")]
        [XmlArrayItem(typeof(ClientCategoryTriggerProperty), ElementName = "ClientCategoryTrigger")]
        [XmlArrayItem(typeof(ClientStatusTransitionTriggerProperty), ElementName = "ClientStatusTransitionTrigger")]
        [XmlArrayItem(typeof(ClientStatusTriggerProperty), ElementName = "ClientStatusTrigger")]
        [XmlArrayItem(typeof(LeadInterestTriggerProperty), ElementName = "LeadInterestTrigger")]
        [XmlArrayItem(typeof(LeadStatusTransitionTriggerProperty), ElementName = "LeadStatusTransitionTrigger")]
        [XmlArrayItem(typeof(PlanProviderTriggerProperty), ElementName = "PlanProviderTrigger")]
        [XmlArrayItem(typeof(PlanStatusTransitionTriggerProperty), ElementName = "PlanStatusTransitionTrigger")]
        [XmlArrayItem(typeof(PlanTypeTriggerProperty), ElementName = "PlanTypeTrigger")]
        [XmlArrayItem(typeof(ServiceCaseCategoryTriggerProperty), ElementName = "ServiceCaseCategoryTrigger")]
        [XmlArrayItem(typeof(ServiceCaseStatusTransitionTriggerProperty), ElementName = "ServiceCaseStatusTransitionTrigger")]
        [XmlArrayItem(typeof(PlanNewBusinessTriggerProperty), ElementName = "PlanNewBusinessTrigger")]
        [XmlArrayItem(typeof(GroupSchemePlanAddedTriggerProperty), ElementName = "GroupSchemePlanAddedTrigger")]
        public List<BaseTriggerProperty> Properties { get; set; }

        public void Add(BaseTriggerProperty triggerProperty)
        {
            if(Properties == null) Properties = new List<BaseTriggerProperty>();
            Properties.Add(triggerProperty);
        }
    }
}