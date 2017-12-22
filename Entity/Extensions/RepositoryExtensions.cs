using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace Material.Data.Entity
{
    public static class RepositoryExtensions
    {
        public static IRepository<TEntity> ToRepository<TEntity>(this IRepository repository) where TEntity : class
        {
            return new GenericRepository<TEntity>(repository);
        }

        public static TEntity GetEntity<TEntity>(this IRepository repository,
            Func<DbQuery<TEntity>, TEntity> query) where TEntity : class
        {
            return repository.Query(query);
        }

        public static List<TEntity> GetEntities<TEntity>(this IRepository repository,
            Func<DbQuery<TEntity>, IEnumerable<TEntity>> query) where TEntity : class
        {
            return repository.Query<TEntity, List<TEntity>>(dbQuery =>
            {
                var result = query(dbQuery);
                return result as List<TEntity> ?? result?.ToList();
            });
        }

        public static List<TEntity> GetEntities<TEntity>(this IRepository repository) where TEntity : class
        {
            return repository.Query<TEntity, List<TEntity>>(dbQuery => dbQuery.ToList());
        }

        public static TEntity GetEntity<TEntity>(this IRepository<TEntity> repository,
            Func<DbQuery<TEntity>, TEntity> query) where TEntity : class
        {
            return repository.Query(query);
        }

        public static List<TEntity> GetEntities<TEntity>(this IRepository<TEntity> repository,
            Func<DbQuery<TEntity>, IEnumerable<TEntity>> query) where TEntity : class
        {
            return repository.Query(dbQuery =>
            {
                var result = query(dbQuery);
                return result as List<TEntity> ?? result?.ToList();
            });
        }

        public static List<TEntity> GetEntities<TEntity>(this IRepository<TEntity> repository) where TEntity : class
        {
            return repository.Query(dbQuery => dbQuery.ToList());
        }
    }
}
