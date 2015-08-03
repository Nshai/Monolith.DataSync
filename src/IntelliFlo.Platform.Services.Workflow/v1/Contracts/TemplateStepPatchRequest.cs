using IntelliFlo.Platform.Http;
using IntelliFlo.Platform.Services.Workflow.Domain;

namespace IntelliFlo.Platform.Services.Workflow.v1.Contracts
{
    public class TemplateStepPatchRequest
    {
        public int? TaskTypeId { get; set; }

        public int? Delay { get; set; }
        public bool? DelayBusinessDays { get; set; }

        [ValidEnumValues(typeof(TaskTransition))]
        public string Transition { get; set; }

        public int? AssignedToPartyId { get; set; }
        public int? AssignedToRoleId { get; set; }

        [ValidEnumValues(typeof(RoleContextType))]
        public string AssignedToRoleContext { get; set; }
    }
}