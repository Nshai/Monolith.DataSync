using System;
using System.ServiceModel;

namespace IntelliFlo.Platform.Services.Workflow.v1
{
    [ServiceContract(Namespace = "http://intelliflo.com/dynamicworkflow/2014/06")]
    public interface IDynamicWorkflow : IDisposable
    {
        [OperationContract()]
        Guid Create(WorkflowContext context);

        [OperationContract(IsOneWay = true)]
        void CreateAsync(WorkflowContext context);

        [OperationContract(IsOneWay = true)]
        void Resume(ResumeContext context);
    }
}
