using System;
using System.Runtime.Serialization;

namespace Microservice.Workflow.v1
{
    [DataContract(Namespace = "http://intelliflo.com/dynamicworkflow/2014/06")]
    public class WorkflowContext
    {
        /// <summary>
        /// Additional context (not used currently)
        /// </summary>
        [DataMember]
        public string AdditionalContext { get; set; }

        /// <summary>
        /// TrustedClient auth bearer token used to populate user context that the instance operates in
        /// </summary>
        [DataMember]
        public string BearerToken { get; set; }

        /// <summary>
        /// Entity Type
        /// </summary>
        [DataMember]
        public string EntityType { get; set; }

        /// <summary>
        /// Entity id
        /// </summary>
        [DataMember]
        public int EntityId { get; set; }

        /// <summary>
        /// Related entity
        /// </summary>
        [DataMember]
        public int RelatedEntityId { get; set; }
        
        /// <summary>
        /// Related client (required for Plan and Service Case workflows)
        /// </summary>
        [DataMember]
        public int ClientId { get; set; }

        /// <summary>
        /// CorrelationId that allows client to query for instance without needing the instance id (which isn't generated until the instance is started)
        /// </summary>
        [DataMember]
        public Guid CorrelationId { get; set; }

        /// <summary>
        /// Whether to prevent creation of duplicate instances of this workflow template for the specified EntityId &amp; RelatedEntityId
        /// </summary>
        [DataMember]
        public bool PreventDuplicates { get; set; }
        
        /// <summary>
        /// Start time for the workflow instance
        /// </summary>
        [DataMember]
        public DateTime Start { get; set; }
    }
}
