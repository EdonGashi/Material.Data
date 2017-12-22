using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using Material.Data.Entity;

namespace Material.Data.InMemory
{
    public class InMemoryRepository : IRepository, IEntityVisitor
    {
        private readonly Dictionary<Type, object> dbSets = new Dictionary<Type, object>();

        public void RegisterDbSet<TEntity>(InMemoryDbSet<TEntity> dbSet) where TEntity : class
        {
            if (dbSet == null)
            {
                throw new ArgumentNullException(nameof(dbSet));
            }

            dbSets.Add(typeof(TEntity), dbSet);
        }

        public void Commit(Entity.Entity entityGraph)
        {
            entityGraph?.Accept(this);
        }

        public TResult Query<TEntity, TResult>(Func<DbQuery<TEntity>, TResult> query) where TEntity : class
        {
            return query(Set<TEntity>());
        }

        public TEntity Find<TEntity>(params object[] keyValues) where TEntity : class
        {
            return Set<TEntity>().Find(keyValues);
        }

        public void Visit<TEntity>(TEntity entity, GraphState state) where TEntity : class
        {
            var dbSet = Set<TEntity>();
            switch (state)
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

        public InMemoryDbSet<TEntity> Set<TEntity>() where TEntity : class
        {
            object set;
            if (dbSets.TryGetValue(typeof(TEntity), out set))
            {
                return (InMemoryDbSet<TEntity>)set;
            }

            var dbSet = new InMemoryDbSet<TEntity>();
            dbSets[typeof(TEntity)] = dbSet;
            return dbSet;
        }
    }
}
