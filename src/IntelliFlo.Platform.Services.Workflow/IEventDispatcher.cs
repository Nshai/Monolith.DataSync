namespace IntelliFlo.Platform.Services.Workflow
{
    public interface IEventDispatcher 
    {
        void Dispatch<T>(T @event) where T : IDomainEvent;
    }
}