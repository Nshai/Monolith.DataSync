using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Markup;
using Microservice.Workflow.Domain;

namespace Microservice.Workflow.v1.Activities
{
    [ContentProperty("Activities")]
    [Designer(typeof (WorkflowStepDesigner))]
    public sealed class WorkflowStep : NativeActivity, ILogActivity
    {
        private readonly Collection<Activity> children;
        private readonly Variable<int> currentIndex;
        private readonly Collection<Variable> variables;
        private readonly Persist persistActivity;

        private CompletionCallback onChildComplete;

        public WorkflowStep()
        {
            children = new Collection<Activity>();
            variables = new Collection<Variable>();
            currentIndex = new Variable<int>();
            persistActivity = new Persist();

            onChildComplete = InternalExecute;
        }

        public InArgument<Guid> StepId { get; set; }
        public InArgument<string> Step { get; set; }
        public InArgument<ILogDetail> StepDetail { get; set; }
        public InArgument<int?> StepIndex { get; set; }

        public InArgument<WorkflowContext> Context { get; set; }
        public InArgument<bool> EnableLogging { get; set; }

        [DependsOn("Variables")]
        public Collection<Activity> Activities
        {
            get { return children; }
        }

        public Collection<Variable> Variables
        {
            get { return variables; }
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
            metadata.AddImplementationVariable(currentIndex);
            metadata.AddImplementationChild(persistActivity);
        }

        protected override void Execute(NativeActivityContext context)
        {
            var stepId = StepId.Get(context);
            if (stepId != Guid.Empty)
                context.Properties.Add(WorkflowConstants.WorkflowStepIdKey, stepId);

            var step = Step.Get(context);
            if (!string.IsNullOrEmpty(step))
                context.Properties.Add(WorkflowConstants.WorkflowStepNameKey, step);

            var workflowContext = Context.Get(context);
            if (workflowContext != null)
                context.Properties.Add(WorkflowConstants.WorkflowContextKey, workflowContext);

            var stepIndex = StepIndex.Get(context);
            if(stepIndex.HasValue)
                context.Properties.Add(WorkflowConstants.WorkflowStepIndexKey, stepIndex);

            Log(context, false);

            context.ScheduleActivity(persistActivity, onChildComplete);
        }

        private void Log(NativeActivityContext context, bool isComplete)
        {
            if (EnableLogging.Get(context))
            {
                LogData data = null;
                var detail = StepDetail.Get(context);
                if (detail != null)
                    data = new LogData() {Detail = detail};

                this.LogMessage(context, isComplete, data);
            }
        }

        private void InternalExecute(NativeActivityContext context, ActivityInstance instance)
        {
            var currentActivityIndex = currentIndex.Get(context);
            if (currentActivityIndex == Activities.Count)
            {
                Log(context, true);
                return;
            }

            var nextChild = Activities[currentActivityIndex];
            context.ScheduleActivity(nextChild, onChildComplete);
            currentIndex.Set(context, ++currentActivityIndex);
        }
    }
}