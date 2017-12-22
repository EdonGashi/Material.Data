using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace Material.Data.Entity
{
    public class EntityDbRepository : IRepository
    {
        private readonly IDbRepository<DbContext> dbRepository;

        public EntityDbRepository(IDbRepository<DbContext> dbRepository)
        {
            this.dbRepository = dbRepository ?? throw new ArgumentNullException(nameof(dbRepository));
        }

        public void Commit(Entity entityGraph) => dbRepository.Commit(entityGraph);

        public TResult Query<TEntity, TResult>(Func<DbQuery<TEntity>, TResult> query) where TEntity : class
            => dbRepository.Query(dbContext => query(dbContext.Set<TEntity>().AsNoTracking()));

        public TEntity Find<TEntity>(params object[] keyValues) where TEntity : class
            => dbRepository.Query(dbContext => dbContext.Set<TEntity>().Find(keyValues));
    }

    public class EntityDbRepository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private readonly IDbRepository<DbContext> dbRepository;

        public EntityDbRepository(IDbRepository<DbContext> dbRepository)
        {
            this.dbRepository = dbRepository ?? throw new ArgumentNullException(nameof(dbRepository));
        }

        public void Commit(EntityContainer<TEntity> entityGraph)
            => dbRepository.Commit(entityGraph);

        public TResult Query<TResult>(Func<DbQuery<TEntity>, TResult> query)
            => dbRepository.Query(dbContext => query(dbContext.Set<TEntity>().AsNoTracking()));

        public TEntity Find(params object[] keyValues)
            => dbRepository.Query(dbContext => dbContext.Set<TEntity>().Find(keyValues));
    }
}
