using System;
using System.Activities;

namespace Microservice.Workflow.Domain
{
    public class CreateTaskStep : IWorkflowStep, IEquatable<CreateTaskStep>
    {
        private readonly Guid id;
        private readonly TaskTransition transition;
        private readonly int taskTypeId;
        private readonly int dueDelay;
        private readonly bool dueDelayBusinessDays;
        private readonly TaskAssignee? assignedTo;
        private readonly int? assignedToPartyId;
        private readonly int? assignedToRoleId;
        private readonly RoleContextType? assignedToRoleContext;
        
        public CreateTaskStep(Guid id, TaskTransition transition, int taskTypeId, TaskAssignee? assignedTo, int dueDelay = 0, bool dueDelayBusinessDays = false, int? assignedToPartyId = null, int? assignedToRoleId = null, RoleContextType? assignedToRoleContext = null)
        {
            this.id = id;
            this.transition = transition;
            this.taskTypeId = taskTypeId;
            this.assignedTo = assignedTo;
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
        
        public TaskAssignee? AssignedTo
        {
            get
            {
                if (assignedTo.HasValue) return assignedTo;
                if (AssignedToRoleContext.HasValue)
                    return TaskAssignee.ContextRole;
                if (AssignedToRoleId.HasValue)
                    return TaskAssignee.Role;
                if (AssignedToPartyId.HasValue)
                    return TaskAssignee.User;
                return assignedTo;
            }
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
                !string.IsNullOrEmpty(request.AssignedTo) ? (TaskAssignee)Enum.Parse(typeof(TaskAssignee), request.AssignedTo) : this.AssignedTo.Value,
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
            return id.Equals(other.id) && transition == other.transition && taskTypeId == other.taskTypeId && dueDelay == other.dueDelay && dueDelayBusinessDays.Equals(other.dueDelayBusinessDays) && assignedTo == other.assignedTo && assignedToPartyId == other.assignedToPartyId && assignedToRoleId == other.assignedToRoleId && assignedToRoleContext == other.assignedToRoleContext;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CreateTaskStep) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = id.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) transition;
                hashCode = (hashCode * 397) ^ taskTypeId;
                hashCode = (hashCode * 397) ^ dueDelay;
                hashCode = (hashCode * 397) ^ dueDelayBusinessDays.GetHashCode();
                hashCode = (hashCode * 397) ^ assignedTo.GetHashCode();
                hashCode = (hashCode * 397) ^ assignedToPartyId.GetHashCode();
                hashCode = (hashCode * 397) ^ assignedToRoleId.GetHashCode();
                hashCode = (hashCode * 397) ^ assignedToRoleContext.GetHashCode();
                return hashCode;
            }
        }
    }
}