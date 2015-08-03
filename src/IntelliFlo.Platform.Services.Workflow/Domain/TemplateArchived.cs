namespace IntelliFlo.Platform.Services.Workflow.Domain
{
    public class TemplateArchived : IDomainEvent
    {
        public int TemplateId { get; set; }
        
        public void Dispatch(IEventDispatcher dispatcher)
        {
            dispatcher.Dispatch(this);
        }
    }
}