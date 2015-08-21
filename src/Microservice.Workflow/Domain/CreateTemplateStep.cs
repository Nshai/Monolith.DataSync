using System;

namespace Microservice.Workflow.Domain
{
    public class CreateTemplateStep
    {
        public string Type { get; set; }

        public int? TaskTypeId { get; set; }
        public Guid? StepId { get; set; }

        public int? Delay { get; set; }
        public bool? DelayBusinessDays { get; set; }
        public string Transition { get; set; }

        public int? AssignedToPartyId { get; set; }
        public int? AssignedToRoleId { get; set; }
        public string AssignedToRoleContext { get; set; }
    }
}
