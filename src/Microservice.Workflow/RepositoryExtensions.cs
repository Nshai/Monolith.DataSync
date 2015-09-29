using IntelliFlo.Platform.NHibernate.Repositories;

namespace Microservice.Workflow
{
    public static class RepositoryExtensions
    {
        public static void SaveWithDispatch<T>(this IRepository<T> repository, T entity, IEventDispatcher dispatcher) where T : class, IDomainObject
        {
            repository.Save(entity);
            foreach (var @event in entity.Events)
            {
                @event.Dispatch(dispatcher);
            }
        }

        public static void DeleteWithDispatch<T>(this IRepository<T> repository, T entity, IEventDispatcher dispatcher) where T : class, IDomainObject
        {
            repository.Delete(entity);
            foreach (var @event in entity.Events)
            {
                @event.Dispatch(dispatcher);
            }
        }
    }
}