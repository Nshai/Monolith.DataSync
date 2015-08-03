using System.Activities;

namespace IntelliFlo.Platform.Services.Workflow.Domain
{
    public interface IMapActivity
    {
        Activity Map(CreateTaskStep step, Template template, int stepIndex);
        Activity Map(DelayStep step, Template template, int stepIndex);
    }
}