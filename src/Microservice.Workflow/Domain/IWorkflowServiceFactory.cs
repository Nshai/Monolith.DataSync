namespace Microservice.Workflow.Domain
{
    public interface IWorkflowServiceFactory 
    {
        string Build(Template template);
    }
}