namespace Microservice.Workflow.Domain
{
    public enum TaskTransition
    {
        Immediately,
        OnProgress,
        OnCompletion
    }
}