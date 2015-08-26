using System.Activities;
using System.ServiceModel;
using Microservice.Workflow.Domain;

namespace Microservice.Workflow.v1.Activities
{
    public sealed class CheckContext : NativeActivity
    {
        public InArgument<WorkflowContext> WorkflowContext { get; set; }
        public InArgument<string> TemplateType { get; set; }

        protected override void Execute(NativeActivityContext context)
        {
            var ctx = WorkflowContext.Get(context);
            var templateType = TemplateType.Get(context);

            if (ctx.EntityId == 0)
            {
                InvalidContext(context, "Entity context was not supplied");
            }

            if (ctx.EntityType != templateType)
            {
                InvalidContext(context, "Template was supplied incorrect context.  Expected {0}, was {1}.", templateType, ctx.EntityType);
            }
        }

        private void InvalidContext(NativeActivityContext context, string message, params object[] args)
        {
            this.LogMessage(context, LogLevel.Error, message, args);
            throw new FaultException(message, new FaultCode(FaultCodes.InvalidContext));
        }
    }
}
