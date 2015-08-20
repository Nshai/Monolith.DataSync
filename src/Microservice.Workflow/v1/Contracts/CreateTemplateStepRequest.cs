using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using IntelliFlo.Platform.Http;
using Microservice.Workflow.Domain;

namespace Microservice.Workflow.v1.Contracts
{
    public class CreateTemplateStepRequest : IValidatableObject
    {
        [Required]
        [ValidEnumValues(typeof (StepType))]
        public string Type { get; set; }

        public int? TaskTypeId { get; set; }
        public Guid? StepId { get; set; }

        public int? Delay { get; set; }
        public bool? DelayBusinessDays { get; set; }
        
        [ValidEnumValues(typeof(TaskTransition))]
        public string Transition { get; set; }

        [ValidEnumValues(typeof(TaskAssignee))]
        public string AssignedTo { get; set; }

        public int? AssignedToPartyId { get; set; }
        public int? AssignedToRoleId { get; set; }

        [ValidEnumValues(typeof(RoleContextType))]
        public string AssignedToRoleContext { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            var stepType = (StepType)Enum.Parse(typeof(StepType), Type);
            switch(stepType)
            {
                case StepType.CreateTask:
                    if(!TaskTypeId.HasValue)
                        results.Add(new ValidationResult("TaskTypeId must be supplied for CreateTask step type"));
                    if (string.IsNullOrEmpty(Transition))
                        results.Add(new ValidationResult("Transition must be supplied for CreateTask step type"));
                    if (string.IsNullOrEmpty(AssignedTo))
                        results.Add(new ValidationResult("AssignedTo must be supplied for CreateTask step type"));
                    var assignedTo = (TaskAssignee)Enum.Parse(typeof (TaskAssignee), AssignedTo);
                    
                    if(assignedTo == TaskAssignee.User && !AssignedToPartyId.HasValue)
                        results.Add(new ValidationResult("AssignedToPartyId must be supplied"));

                    if (assignedTo == TaskAssignee.Role && !AssignedToRoleId.HasValue)
                        results.Add(new ValidationResult("AssignedToRoleId must be supplied"));

                    if (assignedTo == TaskAssignee.ContextRole && string.IsNullOrEmpty(AssignedToRoleContext))
                        results.Add(new ValidationResult("AssignedToRoleContext must be supplied"));
                    
                    break;
            
                case StepType.Delay:
                    if(!Delay.HasValue)
                        results.Add(new ValidationResult("Delay must be supplied for Delay step type"));
                    break;
            }

            return results;
        }
    }
}
