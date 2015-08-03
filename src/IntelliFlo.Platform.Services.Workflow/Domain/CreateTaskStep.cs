using System;
using System.Activities;

namespace IntelliFlo.Platform.Services.Workflow.Domain
{
    public class CreateTaskStep : IWorkflowStep, IEquatable<CreateTaskStep>
    {
        private readonly Guid id;
        private readonly TaskTransition transition;
        private readonly int taskTypeId;
        private readonly int dueDelay;
        private readonly bool dueDelayBusinessDays;
        private readonly int? assignedToPartyId;
        private readonly int? assignedToRoleId;
        private readonly RoleContextType? assignedToRoleContext;

        public CreateTaskStep(Guid id, TaskTransition transition, int taskTypeId, int dueDelay = 0, bool dueDelayBusinessDays = false, int? assignedToPartyId = null, int? assignedToRoleId = null, RoleContextType? assignedToRoleContext = null)
        {
            this.id = id;
            this.transition = transition;
            this.taskTypeId = taskTypeId;
            this.dueDelay = dueDelay;
            this.dueDelayBusinessDays = dueDelayBusinessDays;
            this.assignedToPartyId = assignedToPartyId;
            this.assignedToRoleId = assignedToRoleId;
            this.assignedToRoleContext = assignedToRoleContext;
        }
        
        public Guid Id
        {
            get { return id; }
        }

        public TaskTransition Transition
        {
            get { return transition; }
        }

        public int TaskTypeId
        {
            get { return taskTypeId; }
        }

        public int DueDelay
        {
            get { return dueDelay; }
        }

        public bool DueDelayBusinessDays
        {
            get { return dueDelayBusinessDays; }
        }

        public int? AssignedToPartyId
        {
            get { return assignedToPartyId; }
        }

        public int? AssignedToRoleId
        {
            get { return assignedToRoleId; }
        }

        public RoleContextType? AssignedToRoleContext
        {
            get { return assignedToRoleContext; }
        }

        public Activity GetActivity(IMapActivity mapActivity, Template template, int stepIndex)
        {
            return mapActivity.Map(this, template, stepIndex);
        }

        public IWorkflowStep Patch(TemplateStepPatch request)
        {
            var assigneeUpdated = request.AssignedToPartyId.HasValue || request.AssignedToRoleId.HasValue || !string.IsNullOrEmpty(request.AssignedToRoleContext);

            return new CreateTaskStep(Id,
                !string.IsNullOrEmpty(request.Transition) ? (TaskTransition) Enum.Parse(typeof (TaskTransition), request.Transition) : this.Transition,
                request.TaskTypeId.HasValue ? request.TaskTypeId.Value : TaskTypeId,
                request.Delay.HasValue ? request.Delay.Value : DueDelay,
                request.DelayBusinessDays.HasValue ? request.DelayBusinessDays.Value : DueDelayBusinessDays,
                assigneeUpdated ? request.AssignedToPartyId : AssignedToPartyId,
                assigneeUpdated ? request.AssignedToRoleId : AssignedToRoleId,
                assigneeUpdated ? (!string.IsNullOrEmpty(request.AssignedToRoleContext) ? (RoleContextType) Enum.Parse(typeof (RoleContextType), request.AssignedToRoleContext) : (RoleContextType?) null) : null);
        }

        public bool Equals(CreateTaskStep other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id.Equals(other.Id) && TaskTypeId == other.TaskTypeId && DueDelay == other.DueDelay && DueDelayBusinessDays.Equals(other.DueDelayBusinessDays) && Transition == other.Transition && AssignedToPartyId == other.AssignedToPartyId && AssignedToRoleId == other.AssignedToRoleId && AssignedToRoleContext == other.AssignedToRoleContext;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CreateTaskStep)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Id.GetHashCode();
                hashCode = (hashCode * 397) ^ TaskTypeId;
                hashCode = (hashCode * 397) ^ DueDelay;
                hashCode = (hashCode * 397) ^ DueDelayBusinessDays.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)Transition;
                hashCode = (hashCode * 397) ^ AssignedToPartyId.GetHashCode();
                hashCode = (hashCode * 397) ^ AssignedToRoleId.GetHashCode();
                hashCode = (hashCode * 397) ^ AssignedToRoleContext.GetHashCode();
                return hashCode;
            }
        }
    }
}