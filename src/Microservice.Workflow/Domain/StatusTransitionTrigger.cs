namespace Microservice.Workflow.Domain
{
    public abstract class StatusTransitionTrigger : BaseTrigger
    {
        protected StatusTransitionTrigger(string eventName, WorkflowRelatedTo relatedTo) : base(eventName, relatedTo){}

        public override void PopulateFromRequest(CreateTemplateTrigger request)
        {
            StatusTransition = new StatusTransition(request.StatusTransition.FromStatusId.Value, request.StatusTransition.ToStatusId.Value);
        }

        public override void PopulateDocument(TemplateTrigger document)
        {
            document.StatusTransition = StatusTransition != null ? new TemplateTrigger.StatusTransitionDefinition(){ FromStatusId = StatusTransition.FromStatusId.Value, ToStatusId = StatusTransition.ToStatusId.Value} : null;
        }

        public StatusTransition StatusTransition { get; set; }
    }
}