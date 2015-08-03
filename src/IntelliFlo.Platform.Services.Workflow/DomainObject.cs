using System.Collections.Generic;
using IntelliFlo.Platform.NHibernate;

namespace IntelliFlo.Platform.Services.Workflow
{
    public class DomainObject<TEntity, TId> : EqualityAndHashCodeProvider<TEntity, TId>, IDomainObject
        where TEntity : EqualityAndHashCodeProvider<TEntity, TId>
    {
        private readonly List<IDomainEvent> events = new List<IDomainEvent>();

        public virtual IReadOnlyList<IDomainEvent> Events
        {
            get { return events.AsReadOnly(); }
        }

        protected void RaiseEvent(IDomainEvent @event)
        {
            events.Add(@event);
        }
    }
}
