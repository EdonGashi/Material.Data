using System;
using System.Data.Entity.Infrastructure;

namespace Material.Data.Entity
{
    public class GenericRepository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private readonly IRepository repository;

        public GenericRepository(IRepository repository)
        {
            this.repository = repository;
        }

        public void Commit(EntityContainer<TEntity> entityGraph) => repository.Commit(entityGraph);

        public TResult Query<TResult>(Func<DbQuery<TEntity>, TResult> query) => repository.Query(query);

        public TEntity Find(params object[] keyValues) => repository.Find<TEntity>(keyValues);
    }
}
