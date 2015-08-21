namespace Microservice.Workflow
{
    public interface IHandle<in T>
        where T : IDomainEvent
    {
        void Handle(T @event);
    }
}
