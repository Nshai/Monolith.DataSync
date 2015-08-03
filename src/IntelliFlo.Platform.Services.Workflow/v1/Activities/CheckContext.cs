using System.Activities;
using System.ServiceModel;
using IntelliFlo.Platform.Services.Workflow.Domain;

namespace IntelliFlo.Platform.Services.Workflow.v1.Activities
{
    public sealed class CheckContext : CodeActivity
    {
        public InArgument<WorkflowContext> WorkflowContext { get; set; }
        public InArgument<string> TemplateType { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            var ctx = WorkflowContext.Get(context);
            var templateType = TemplateType.Get(context);

            if (ctx.EntityId == 0)
                throw new FaultException("Entity context was not supplied", new FaultCode(FaultCodes.InvalidContext));

            if (ctx.EntityType != templateType)
                throw new FaultException(string.Format("Template was supplied incorrect context.  Expected {0}, was {1}.", templateType, ctx.EntityType), new FaultCode(FaultCodes.InvalidContext));
        }
    }
}
