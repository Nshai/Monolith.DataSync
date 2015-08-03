using System;

namespace IntelliFlo.Platform.Services.Workflow.Collaborators.v1
{
    public class CreateTaskRequest
    {
        public int? AssignedToPartyId { get; set; }
        public int? AssignedToRoleId { get; set; }
        public int AssignedByPartyId { get; set; }
        public int TaskTypeId { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; }
        public int? RelatedPartyId { get; set; }
        public int? RelatedJointPartyId { get; set; }
        public int? RelatedPlanId { get; set; }
        public int? RelatedServiceCaseId { get; set; }
    }
}
