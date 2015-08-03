namespace IntelliFlo.Platform.Services.Workflow.Domain
{
    public class TemplateCloned : IDomainEvent
    {
        public int TemplateId { get; set; }
        public int ClonedFromTemplateId { get; set; }

        public void Dispatch(IEventDispatcher dispatcher)
        {
            dispatcher.Dispatch(this);
        }
    }
}