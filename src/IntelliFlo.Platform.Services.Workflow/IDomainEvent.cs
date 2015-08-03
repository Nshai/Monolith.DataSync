namespace IntelliFlo.Platform.Services.Workflow
{
    public interface IDomainEvent
    {
        void Dispatch(IEventDispatcher dispatcher);
    }
}
