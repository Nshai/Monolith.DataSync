using System.Collections.Generic;
using Autofac;

namespace Microservice.Workflow
{
    public class EventDispatcher : IEventDispatcher
    {
        private readonly ILifetimeScope lifetimeScope;

        public EventDispatcher(ILifetimeScope lifetimeScope)
        {
            this.lifetimeScope = lifetimeScope;
        }

        public void Dispatch<T>(T @event) where T : IDomainEvent
        {
            foreach (var handler in lifetimeScope.Resolve<IEnumerable<IHandle<T>>>())
            {
                handler.Handle(@event);
            }
        }
    }
}
