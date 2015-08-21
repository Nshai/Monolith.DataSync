using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Microservice.Workflow.Domain
{
    [Serializable]
    public class WorkflowDefinition : IEquatable<WorkflowDefinition>
    {
        public WorkflowDefinition()
        {
            StepsInternal = new List<IWorkflowStep>();
        }

        [JsonIgnore]
        public IReadOnlyList<IWorkflowStep> Steps
        {
            get { return StepsInternal.AsReadOnly(); }
        }

        public void AddStep(IWorkflowStep step)
        {
            StepsInternal.Add(step);
        }

        public void DeleteStep(IWorkflowStep step)
        {
            StepsInternal.Remove(step);
        }

        public void MoveStepUp(IWorkflowStep step)
        {
            var index = StepsInternal.IndexOf(step);
            if (index == 0)
                return;
            StepsInternal.Remove(step);
            StepsInternal.Insert(--index, step);
        }

        public void MoveStepDown(IWorkflowStep step)
        {
            var index = StepsInternal.IndexOf(step);
            if (index == StepsInternal.Count - 1)
                return;
            StepsInternal.Remove(step);
            StepsInternal.Insert(++index, step);
        }

        public void UpdateStep(IWorkflowStep step, IWorkflowStep updatedStep)
        {
            var index = StepsInternal.IndexOf(step);
            StepsInternal.Remove(step);
            StepsInternal.Insert(index, updatedStep);
        }

        [JsonProperty("Steps")]
        [JsonConverter(typeof(JsonMappedToTypeNameTypeListConverter<IWorkflowStep, CreateTaskStep>))]
        private List<IWorkflowStep> StepsInternal { get; set; }

        public bool Equals(WorkflowDefinition other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Steps.SequenceEqual(other.Steps);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((WorkflowDefinition)obj);
        }

        public override int GetHashCode()
        {
            return (Steps != null ? Steps.GetHashCode() : 0);
        }
    }
}
