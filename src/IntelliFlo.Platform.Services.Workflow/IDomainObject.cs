using System.Collections.Generic;

namespace IntelliFlo.Platform.Services.Workflow
{
    public interface IDomainObject
    {
        IReadOnlyList<IDomainEvent> Events { get; }
    }
}