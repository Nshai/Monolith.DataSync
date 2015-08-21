using System.Activities;

namespace Microservice.Workflow.Domain
{
    public interface IMapActivity
    {
        Activity Map(CreateTaskStep step, Template template, int stepIndex);
        Activity Map(DelayStep step, Template template, int stepIndex);
    }
}