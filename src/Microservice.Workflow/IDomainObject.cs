using System.Collections.Generic;

namespace Microservice.Workflow
{
    public interface IDomainObject
    {
        IReadOnlyList<IDomainEvent> Events { get; }
    }
}