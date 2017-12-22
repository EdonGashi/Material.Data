using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using Material.Data.Entity;

namespace Material.Data.InMemory
{
    public class InMemoryRepository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private readonly InMemoryDbSet<TEntity> dbSet;

        public InMemoryRepository()
            : this(null)
        {
        }

        public InMemoryRepository(IEnumerable<TEntity> entities)
        {
            dbSet = new InMemoryDbSet<TEntity>(entities);
        }

        public void Commit(EntityContainer<TEntity> entityGraph)
        {
            if (entityGraph == null)
            {
                return;
            }

            {
                var entity = entityGraph as Entity<TEntity>;
                if (entity != null)
                {
                    UpdateEntity(entity);
                    return;
                }
            }

            var entityCollection = entityGraph as EntityCollection<TEntity>;
            if (entityCollection != null)
            {
                foreach (var entity in entityCollection.Entities)
                {
                    UpdateEntity(entity);
                }
            }
        }

        public TResult Query<TResult>(Func<DbQuery<TEntity>, TResult> query)
        {
            return query(dbSet);
        }

        public TEntity Find(params object[] keyValues)
        {
            return dbSet.Find(keyValues);
        }

        private void UpdateEntity(Entity<TEntity> entity)
        {
            switch (entity.State)
            {
                case GraphState.Added:
                    dbSet.Add(entity);
                    return;
                case GraphState.Deleted:
                    dbSet.Remove(entity);
                    return;
                case GraphState.Unchanged:
                    if (!dbSet.Contains(entity))
                    {
                        dbSet.Add(entity);
                    }

                    return;
                case GraphState.Modified:
                    return;
                default:
                    return;
            }
        }
    }
}
