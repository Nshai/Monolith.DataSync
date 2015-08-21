using System.Configuration;

namespace Microservice.Workflow.Domain
{
    public class WorkflowConfiguration : ConfigurationSection, IWorkflowConfiguration
    {
        public const string SectionXPath = "WorkflowConfiguration";
        public static WorkflowConfiguration GetSection()
        {
            return (WorkflowConfiguration)ConfigurationManager.GetSection(SectionXPath);
        }

        private const string EndpointAddressKey = "EndpointAddress";
        [ConfigurationProperty(EndpointAddressKey, IsRequired = false)]
        public virtual string EndpointAddress
        {
            get
            {
                return (string)this[EndpointAddressKey];
            }
            set
            {
                this[EndpointAddressKey] = value;
            }
        }
    }
}
