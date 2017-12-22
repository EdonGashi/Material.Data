using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace Material.Data.Entity
{
    public static class DbRepositoryEntityExtensions
    {
        public static IRepository ToRepository(this IDbRepository<DbContext> dbRepository)
        {
            return new EntityDbRepository(dbRepository);
        }

        public static IRepository<TEntity> ToRepository<TEntity>(this IDbRepository<DbContext> dbRepository)
            where TEntity : class
        {
            return new EntityDbRepository<TEntity>(dbRepository);
        }

        public static TEntity Find<TEntity>(this IDbRepository<DbContext> dbRepository, params object[] keyValues) where TEntity : class
        {
            return dbRepository.Query(context => context.Set<TEntity>().Find(keyValues));
        }

        public static void Commit(this IDbRepository<DbContext> dbRepository, Entity entityGraph)
        {
            if (entityGraph == null)
            {
                return;
            }

            dbRepository.Transaction(context =>
            {
                context.Configuration.AutoDetectChangesEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                entityGraph.Commit(context);
            });
        }

        public static TEntity GetEntity<TEntity>(this IDbRepository<DbContext> dbRepository,
            Func<DbQuery<TEntity>, TEntity> query) where TEntity : class
        {
            return dbRepository.Query(dbContext => query(dbContext.Set<TEntity>().AsNoTracking()));
        }

        public static List<TEntity> GetEntities<TEntity>(this IDbRepository<DbContext> dbRepository,
            Func<DbQuery<TEntity>, IEnumerable<TEntity>> query) where TEntity : class
        {
            return dbRepository.Query(dbContext =>
            {
                var result = query(dbContext.Set<TEntity>().AsNoTracking());
                return result as List<TEntity> ?? result?.ToList();
            });
        }

        public static List<TEntity> GetEntities<TEntity>(this IDbRepository<DbContext> dbRepository)
            where TEntity : class
        {
            return dbRepository.Query(dbContext => dbContext.Set<TEntity>().AsNoTracking().ToList());
        }
    }
}
