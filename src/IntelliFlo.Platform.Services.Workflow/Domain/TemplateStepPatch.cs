﻿namespace IntelliFlo.Platform.Services.Workflow.Domain
{
    public class TemplateStepPatch
    {
        public int? TaskTypeId { get; set; }

        public int? Delay { get; set; }
        public bool? DelayBusinessDays { get; set; }

        public string Transition { get; set; }

        public int? AssignedToPartyId { get; set; }
        public int? AssignedToRoleId { get; set; }

        public string AssignedToRoleContext { get; set; }
    }
}
