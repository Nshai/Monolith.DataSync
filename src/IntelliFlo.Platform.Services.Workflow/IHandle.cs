﻿namespace IntelliFlo.Platform.Services.Workflow
{
    public interface IHandle<in T>
        where T : IDomainEvent
    {
        void Handle(T @event);
    }
}
