using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace Material.Data.Entity
{
    public static class DbContextExtensions
    {
        public static DbQuery<TEntity> NoTracking<TEntity>(this DbContext dbContext) where TEntity : class
        {
            return dbContext.Set<TEntity>().AsNoTracking();
        }

        public static TEntity Find<TEntity>(this DbContext dbContext, params object[] keyValues) where TEntity : class
        {
            return dbContext.Set<TEntity>().Find(keyValues);
        }

        public static TEntity GetEntity<TEntity>(this DbContext dbContext,
            Func<DbQuery<TEntity>, TEntity> query) where TEntity : class
        {
            return query(dbContext.Set<TEntity>().AsNoTracking());
        }

        public static List<TEntity> GetEntities<TEntity>(this DbContext dbContext,
            Func<DbQuery<TEntity>, IEnumerable<TEntity>> query) where TEntity : class
        {
            var result = query(dbContext.Set<TEntity>().AsNoTracking());
            return result as List<TEntity> ?? result?.ToList();
        }

        public static List<TEntity> GetEntities<TEntity>(this DbContext dbContext)
            where TEntity : class
        {
            return dbContext.Set<TEntity>().AsNoTracking().ToList();
        }
    }
}
