using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using IntelliFlo.Platform;
using IntelliFlo.Platform.Principal;

namespace Microservice.Workflow.Domain
{
    public class Template : DomainObject<Template, int>
    {
        private IList<TemplateVersion> versions;
        private int tenantId;
        private string name;
        private readonly DateTime createdDate;
        private readonly WorkflowRelatedTo relatedTo;
        private long concurrencyId;
        private TemplateCategory category;
        private string status;

        protected Template() {}
        public Template(string name, int tenantId, TemplateCategory category, WorkflowRelatedTo relatedTo, int ownerUserId)
        {
            this.name = name;
            this.tenantId = tenantId;
            this.category = category;
            this.relatedTo = relatedTo;
            createdDate = DateTime.UtcNow;

            versions = new List<TemplateVersion>
            {
                new TemplateVersion(this)
            };

            this.OwnerUserId = ownerUserId;
        }

        public virtual string Name
        {
            get { return name; }
            set
            {
                IsValidForUpdate();
                name = value; 
            }
        }

        public virtual DateTime CreatedDate
        {
            get { return createdDate; }
        }

        public virtual WorkflowRelatedTo RelatedTo
        {
            get { return relatedTo; }
        }

        public virtual int TenantId
        {
            get { return tenantId; }
            set
            {
                tenantId = value;
                CurrentVersion.TenantId = value;
            }
        }

        public virtual long ConcurrencyId
        {
            get { return concurrencyId; }
        }

        public virtual TemplateCategory Category
        {
            get { return category; }
            set
            {
                IsValidForUpdate();
                category = value; 
            }
        }
        
        public virtual IList<TemplateVersion> Versions
        {
            get { return versions; }
            set { versions = value; }
        }

        public virtual TemplateVersion CurrentVersion
        {
            get
            {
                return Versions.OrderByDescending(v => v.CreatedDate).FirstOrDefault();
            }
        }
        
        public virtual Guid Guid
        {
            get { return CurrentVersion.Guid; }
        }

        public virtual int OwnerUserId
        {
            get { return CurrentVersion.OwnerUserId; }
            set
            {
                IsValidForUpdate();
                CurrentVersion.OwnerUserId = value; 
            }
        }

        public virtual int? ApplicableToGroupId
        {
            get { return CurrentVersion.ApplicableToGroupId; }
            set
            {
                if (value == CurrentVersion.ApplicableToGroupId) return;
                IsValidForUpdate();

                CurrentVersion.ApplicableToGroupId = value;
                RaiseEvent(new TemplateGroupUpdated()
                {
                    TemplateId = Id
                });
            }
        }

        public virtual bool? IncludeSubGroups
        {
            get { return CurrentVersion.IncludeSubGroups; }
            set
            {
                IsValidForUpdate();
                CurrentVersion.IncludeSubGroups = value;
            }
        }

        public virtual string Notes
        {
            get { return CurrentVersion.Notes; }
            set { CurrentVersion.Notes = value; }
        }

        public virtual WorkflowStatus Status
        {
            get
            {
                return !string.IsNullOrEmpty(status) ? (WorkflowStatus)Enum.Parse(typeof(WorkflowStatus), status) : CurrentVersion.Status;
            }
        }

        public virtual WorkflowDefinition Definition
        {
            get { return CurrentVersion.Definition; }
            protected internal set { CurrentVersion.Definition = value; }
        }

        public virtual IList<TemplateRole> Roles
        {
            get { return CurrentVersion.Roles; }
        }

        public virtual TemplateTriggerSet TriggerSet
        {
            get { return CurrentVersion.Triggers.SingleOrDefault() ?? new TemplateTriggerSet(CurrentVersion, TenantId, TriggerType.None); }
        }

        public virtual bool InUse
        {
            get { return CurrentVersion.InUse; }
        }
        
        public virtual bool IsActive
        {
            get { return Status == WorkflowStatus.Active || InUse; }
        }

        public virtual void SetRoles(IEnumerable<int> roleIds)
        {
            IsValidForUpdate();

            var roles = roleIds.Select(r => new TemplateRole
            {
                RoleId = r,
                TemplateVersion = CurrentVersion,
                TenantId = Thread.CurrentPrincipal.AsIFloPrincipal().TenantId
            }).ToList();
            CurrentVersion.Roles.Clear();
            CurrentVersion.Roles.AddRange(roles);
        }

        public virtual IReadOnlyList<IWorkflowStep> Steps
        {
            get { return Definition.Steps; }
        }

        public virtual IWorkflowStep UpdateStep(Guid stepId, TemplateStepPatch patch)
        {
            IsValidForUpdate();

            var step = Definition.Steps.SingleOrDefault(s => s.Id == stepId);
            if (step == null)
                throw new TemplateStepNotFoundException();

            var updatedStep = step.Patch(patch);
            Definition.UpdateStep(step, updatedStep);
            return updatedStep;
        }

        public virtual void AddStep(IWorkflowStep step)
        {
            IsValidForUpdate();
            Definition.AddStep(step);
        }

        public virtual void DeleteStep(Guid stepId)
        {
            IsValidForUpdate();

            var step = Definition.Steps.SingleOrDefault(s => s.Id == stepId);
            if (step == null)
                throw new TemplateStepNotFoundException();

            Definition.DeleteStep(step);
        }

        public virtual IWorkflowStep MoveStepUp(Guid stepId)
        {
            IsValidForUpdate();

            var step = Definition.Steps.SingleOrDefault(s => s.Id == stepId);
            if (step == null)
                throw new TemplateStepNotFoundException();

            Definition.MoveStepUp(step);

            return step;
        }

        public virtual IWorkflowStep MoveStepDown(Guid stepId)
        {
            IsValidForUpdate();

            var step = Definition.Steps.SingleOrDefault(s => s.Id == stepId);
            if (step == null)
                throw new TemplateStepNotFoundException();

            Definition.MoveStepDown(step);

            return step;
        }

        public virtual void SetTrigger(TriggerType type, BaseTrigger trigger)
        {
            IsValidForUpdate();

            var triggerSet = new TemplateTriggerSet(CurrentVersion, TenantId, type) {Trigger = trigger};

            Check.IsTrue(!trigger.RelatedTo.HasValue || trigger.RelatedTo == RelatedTo, "Trigger type is not valid for this template");

            CurrentVersion.Triggers.Clear();
            CurrentVersion.Triggers.Add(triggerSet);
        }

        public virtual bool IsUserPermittedToRun(int activeRoleId, int[] groupIds, bool isTriggeredInstance)
        {
            if (ApplicableToGroupId.HasValue)
            {
                if (!groupIds.Any()) return false;

                if (IncludeSubGroups.HasValue && IncludeSubGroups.Value)
                {
                    if (!groupIds.Contains(ApplicableToGroupId.Value))
                        return false;
                }
                else
                {
                    if (groupIds[0] != ApplicableToGroupId)
                        return false;
                }
            }

            return isTriggeredInstance || CurrentVersion.Roles.Any(r => r.RoleId == activeRoleId);
        }

        public virtual void MarkForDeletion()
        {
            if(InUse)
                throw new TemplateNotUpdatableException(Status == WorkflowStatus.Archived);

            var version = versions.Single();
            if (version.Status == WorkflowStatus.Active)
            {
                RaiseEvent(new TemplateMadeInactive
                {
                    TemplateId = Id
                });
            }
        }

        public virtual void SetStatus(WorkflowStatus status)
        {
            var currentStatus = Status;
            if (status == currentStatus)
                return;
            
            if(InUse && status != WorkflowStatus.Archived)
                throw new TemplateNotUpdatableException();

            this.status = status.ToString();
            CurrentVersion.Status = status;
            switch (status)
            {
                case WorkflowStatus.Draft:
                    if (currentStatus == WorkflowStatus.Active)
                        RaiseEvent(new TemplateMadeInactive { TemplateId = Id });
                    break;
                case WorkflowStatus.Active:
                    RaiseEvent(new TemplateMadeActive { TemplateId = Id });
                    break;
                case WorkflowStatus.Archived:
                    if (currentStatus == WorkflowStatus.Active)
                        RaiseEvent(new TemplateMadeInactive { TemplateId = Id });
                    RaiseEvent(new TemplateArchived { TemplateId = Id });
                    break;
            }
        }

        private void IsValidForUpdate()
        {
            if (IsActive || Status == WorkflowStatus.Archived)
                throw new TemplateNotUpdatableException(Status == WorkflowStatus.Archived);
        }
    }
}