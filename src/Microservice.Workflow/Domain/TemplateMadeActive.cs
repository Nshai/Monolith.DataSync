namespace Microservice.Workflow.Domain
{
    public class TemplateMadeActive : IDomainEvent
    {
        public int TemplateId { get; set; }

        public void Dispatch(IEventDispatcher dispatcher)
        {
            dispatcher.Dispatch(this);
        }
    }
}