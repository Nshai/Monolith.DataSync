using System;

namespace Microservice.Workflow.Collaborators.v1
{
    public class TaskDocument
    {
        public int TaskId { get; set; }
        public int? AssignedToPartyId { get; set; }
        public int? AssignedToRoleId { get; set; }
        public int AssignedByPartyId { get; set; }
        public string TaskCategory { get; set; }
        public string TaskType { get; set; }
        public string Subject { get; set; }
        public bool IsSharedWithClient { get; set; }
        public string Priority { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public int PercentageComplete { get; set; }
        public string Status { get; set; }
        public int EstimatedTimeHrs { get; set; }
        public int EstimatedTimeMins { get; set; }
        public bool ShowInDiary { get; set; }
        public int? RelatedPartyId { get; set; }
        public int? RelatedJointPartyId { get; set; }
        public int? RelatedPlanId { get; set; }
        public string RelatedPlanDescription { get; set; }
        public int? RelatedServiceCaseId { get; set; }
    }
}
