using System;
using System.ServiceModel;

namespace Microservice.Workflow.v1
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
