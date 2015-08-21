using System.Activities;

namespace Microservice.Workflow.v1.Activities
{
    public interface IEntityTaskBuilderFactory
    {
        EntityTaskBuilder Get(string type, Activity parentActivity, NativeActivityContext context);
    }
}