using System;
using System.Data.Entity.Infrastructure;

namespace Material.Data.Entity
{
    public interface IRepository
    {
        void Commit(Entity entityGraph);

        TResult Query<TEntity, TResult>(Func<DbQuery<TEntity>, TResult> query) where TEntity : class;

        TEntity Find<TEntity>(params object[] keyValues) where TEntity : class;
    }

    public interface IRepository<TEntity> where TEntity : class
    {
        void Commit(EntityContainer<TEntity> entityGraph);

        TResult Query<TResult>(Func<DbQuery<TEntity>, TResult> query);

        TEntity Find(params object[] keyValues);
    }
}
