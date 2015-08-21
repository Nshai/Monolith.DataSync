using System.Activities.Tracking;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Activities;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using NHibernate;

namespace Microservice.Workflow.Engine
{
    public class DatabaseTrackingBehavior : IServiceBehavior
    {
        public virtual void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            var workflowServiceHost = serviceHostBase as WorkflowServiceHost;
            if (workflowServiceHost == null) return;

            var worflowServiceHost = serviceHostBase as WorkflowServiceHost;
            var trackingProfile = GetProfile();

            var sessionFactory = IoC.Resolve<ISessionFactory>(Constants.ContainerId);
            using (var session = sessionFactory.OpenSession())
            {
                worflowServiceHost.WorkflowExtensions.Add(() => new DatabaseTrackingParticipant(sessionFactory) {TrackingProfile = trackingProfile});
            }
        }

        public virtual void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters) {}
        public virtual void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase) {}

        private TrackingProfile GetProfile()
        {
            var profile = new TrackingProfile() {Name = "Database Tracking Profile"};
            profile.Queries.Add(new WorkflowInstanceQuery() { States = { "*" } });
            profile.Queries.Add(new CustomTrackingQuery(){ Name = "*", ActivityName = "*"});
            profile.ImplementationVisibility = ImplementationVisibility.All;
            return profile;
        }
    }
}