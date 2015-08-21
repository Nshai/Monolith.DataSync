namespace Microservice.Workflow
{
    public interface IDomainEvent
    {
        void Dispatch(IEventDispatcher dispatcher);
    }
}
