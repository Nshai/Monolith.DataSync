using IntelliFlo.Platform.Bus;
using Microservice.Workflow.Messaging.Messages;
using Microservice.Workflow.v1;
using System.Threading.Tasks;

namespace Microservice.Workflow.Messaging.MessageHandlers
{
    public class ResumeInstanceHandler : IMessageHandlerAsync<ResumeInstance>
    {
        private readonly IInstanceResource instanceResource;

        public ResumeInstanceHandler(IInstanceResource instanceResource)
        {
            this.instanceResource = instanceResource;
        }

        public Task<bool> Handle(IBusContext context, ResumeInstance message)
        {
            instanceResource.Resume(message.InstanceId, message.Bookmark);
            return Task.FromResult(true);
        }
    }
}
