using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using IntelliFlo.Platform.Http;
using IntelliFlo.Platform.Services.Workflow.Domain;

namespace IntelliFlo.Platform.Services.Workflow.v1.Contracts
{
    public class CreateInstanceRequest : IValidatableObject
    {
        public CreateInstanceRequest()
        {
            CorrelationId = GuidCombGenerator.Generate();
        }

        [Required]
        [ValidEnumValues(typeof(EntityType))]
        public string EntityType { get; set; }

        [Required]
        public int EntityId { get; set; }

        public int RelatedEntityId { get; set; }
        public int ClientId { get; set; }
        public Guid CorrelationId { get; set; }
        public DateTime? Start { get; set; }
        public string AdditionalContext { get; set; }
        public bool PreventDuplicates { get; set; }

        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
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