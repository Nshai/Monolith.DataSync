using IntelliFlo.Platform.NHibernate.Repositories;

namespace IntelliFlo.Platform.Services.Workflow
{
    public static class RepositoryExtensions
    {
        public static void SaveWithDispatch<T>(this IRepository<T> repository, T entity, IEventDispatcher dispatcher) where T : class, IDomainObject
        {
            foreach (var @event in entity.Events)
            {
                @event.Dispatch(dispatcher);
            }
            repository.Save(entity);
        }

        public static void DeleteWithDispatch<T>(this IRepository<T> repository, T entity, IEventDispatcher dispatcher) where T : class, IDomainObject
        {
            foreach (var @event in entity.Events)
            {
                @event.Dispatch(dispatcher);
            }
            repository.Delete(entity);
        }
    }
}