using System;
using System.Activities;

namespace IntelliFlo.Platform.Services.Workflow.Domain
{
    public class DelayStep : IWorkflowStep, IEquatable<DelayStep>
    {
        private readonly Guid id;
        private readonly int days;
        private readonly bool businessDays;

        public DelayStep(Guid id, int days = 0, bool businessDays = false)
        {
            this.id = id;
            this.days = days;
            this.businessDays = businessDays;
        }

        public Guid Id
        {
            get { return id; }
        }

        public int Days
        {
            get { return days; }
        }

        public bool BusinessDays
        {
            get { return businessDays; }
        }

        public Activity GetActivity(IMapActivity mapActivity, Template template, int stepIndex)
        {
            return mapActivity.Map(this, template, stepIndex);
        }

        public IWorkflowStep Patch(TemplateStepPatch request)
        {
            return new DelayStep(Id, 
                request.Delay.HasValue ? request.Delay.Value : Days,
                request.DelayBusinessDays.HasValue ? request.DelayBusinessDays.Value : BusinessDays);
        }

        public bool Equals(DelayStep other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id.Equals(other.Id) && Days == other.Days && BusinessDays.Equals(other.BusinessDays);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DelayStep)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Id.GetHashCode();
                hashCode = (hashCode * 397) ^ Days;
                hashCode = (hashCode * 397) ^ BusinessDays.GetHashCode();
                return hashCode;
            }
        }
    }
}