using IntelliFlo.Platform.Bus;
using Microservice.Workflow.Messaging.Messages;
using Microservice.Workflow.v1;

namespace Microservice.Workflow.Messaging.MessageHandlers
{
    public class ResumeInstanceHandler : IMessageHandler<ResumeInstance>
    {
        private readonly IInstanceResource instanceResource;

        public ResumeInstanceHandler(IInstanceResource instanceResource)
        {
            this.instanceResource = instanceResource;
        }

        public bool Handle(IBusContext context, ResumeInstance message)
        {
            instanceResource.Resume(message.InstanceId, message.Bookmark);
            return true;
        }
    }
}
