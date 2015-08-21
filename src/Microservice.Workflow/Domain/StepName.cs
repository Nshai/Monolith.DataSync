namespace Microservice.Workflow.Domain
{
    public enum StepName
    {
        Undefined,
        Created,
        CreateTask,
        Delay,
        Errored,
        Aborted,
        Completed
    }
}
