using IntelliFlo.Platform.Http;
using Microservice.Workflow.Domain;

namespace Microservice.Workflow.v1.Contracts
{
    public class TemplatePatchRequest
    {
        public string Name { get; set; }
        public int? TemplateCategoryId { get; set; }
        public int? OwnerUserId { get; set; }
        public bool? ApplicableToGroup { get; set; }
        public int? ApplicableToGroupId { get; set; }
        public bool? IncludeSubGroups { get; set; }

        [ValidEnumValues(typeof (WorkflowStatus))]
        public string Status { get; set; }

        public string Notes { get; set; }
    }
}