namespace Microservice.Workflow
{
    public interface IEventDispatcher 
    {
        void Dispatch<T>(T @event) where T : IDomainEvent;
    }
}