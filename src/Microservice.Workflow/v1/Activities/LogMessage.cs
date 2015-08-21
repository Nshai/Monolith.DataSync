using System.Activities;
using Microservice.Workflow.Domain;

namespace Microservice.Workflow.v1.Activities
{
    public sealed class LogMessage : NativeActivity, ILogActivity
    {
        public InArgument<ILogDetail> StepDetail { get; set; }
        public InArgument<bool> IsComplete { get; set; }

        // If your activity returns a value, derive from CodeActivity<TResult>
        // and return the value from the Execute method.
        protected override void Execute(NativeActivityContext context)
        {
            var isComplete = IsComplete.Get(context);

            LogData data = null;
            var detail = StepDetail.Get(context);
            if (detail != null)
                data = new LogData() { Detail = detail };

            this.LogMessage(context, isComplete, data);
        }
    }
}
