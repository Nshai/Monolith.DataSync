using System;
using System.Runtime.Serialization;

namespace Microservice.Workflow.v1
{
    [DataContract(Namespace = "http://intelliflo.com/dynamicworkflow/2014/06")]
    public class ResumeContext
    {
        [DataMember]
        public Guid InstanceId { get; set; }
        [DataMember]
        public string BookmarkName { get; set; }
        [DataMember]
        public string AdditionalContext { get; set; }
    }
}
