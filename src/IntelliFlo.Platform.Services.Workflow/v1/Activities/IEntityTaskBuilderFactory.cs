using System.Activities;

namespace IntelliFlo.Platform.Services.Workflow.v1.Activities
{
    public interface IEntityTaskBuilderFactory
    {
        EntityTaskBuilder Get(string type, Activity parentActivity, NativeActivityContext context);
    }
}