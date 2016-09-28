using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using IntelliFlo.Platform.Http;
using Microservice.Workflow.Domain;
using Microservice.Workflow.v1.Activities;
using Newtonsoft.Json;

namespace Microservice.Workflow.v1.Contracts
{
    public class MigratedInstanceRequest : IValidatableObject
    {
        [Required]
        [ValidEnumValues(typeof(EntityType))]
        public string EntityType { get; set; }

        [Required]
        public int EntityId { get; set; }

        [Required]
        public int? UserId { get; set; }

        [Required]
        public Guid? Subject { get; set; }

        [Required]
        public int? TenantId { get; set; }

        [Required]
        public RunToDefinition RunTo { get; set; }

        [Required]
        public Guid? MigratedInstanceId { get; set; }
        
        public int RelatedEntityId { get; set; }
        public int ClientId { get; set; }
        public DateTime? Start { get; set; }
        public bool PreventDuplicates { get; set; }

        public string AdditionalContext
        {
            get
            {
                return JsonConvert.SerializeObject(new AdditionalContext()
                {
                    RunTo = new RunToAdditionalContext()
                    {
                        StepId = RunTo.StepId,
                        StepIndex = RunTo.StepIndex,
                        TaskId = RunTo.TaskId,
                        DelayTime = RunTo.DelayTime
                    }
                });
            }
        }

        public class RunToDefinition
        {
            public Guid? StepId { get; set; }
            public int StepIndex { get; set; }
            public int? TaskId { get; set; }
            public DateTime? DelayTime { get; set; }
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            EntityType entityType;
            if (!Enum.TryParse(EntityType, false, out entityType))
            {
                results.Add(new ValidationResult("EntityType not recognised"));
            }
            else
            {
                if (entityType != Domain.EntityType.Plan && entityType != Domain.EntityType.ServiceCase) return results;
                if (ClientId == 0)
                    results.Add(new ValidationResult("ClientId must be supplied with Plan and ServiceCase entity types"));
            }

            return results;
        }
    }

}