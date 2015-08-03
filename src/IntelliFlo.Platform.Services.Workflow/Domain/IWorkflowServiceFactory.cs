namespace IntelliFlo.Platform.Services.Workflow.Domain
{
    public interface IWorkflowServiceFactory 
    {
        string Build(Template template);
    }
}