using System.Activities.Tracking;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Activities;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;

namespace Microservice.Workflow.Engine
{
    public class InstanceCountBehavior : IServiceBehavior
    {
        private readonly IWorkflowHost host;

        public InstanceCountBehavior(IWorkflowHost host)
        {
            this.host = host;
        }

        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            var workflowServiceHost = serviceHostBase as WorkflowServiceHost;
            if (workflowServiceHost == null) return;

            var trackingProfile = GetProfile();

            workflowServiceHost.WorkflowExtensions.Add(() => new InstanceCountParticipant(host) { TrackingProfile = trackingProfile });
        }

        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters){}
        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase) {}

        private TrackingProfile GetProfile()
        {
            var profile = new TrackingProfile() { Name = "Instance Count Tracking Profile" };
            profile.Queries.Add(new WorkflowInstanceQuery() { States = { "*" } });
            profile.ImplementationVisibility = ImplementationVisibility.All;
            return profile;
        }
    }
}
