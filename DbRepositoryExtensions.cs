using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace Material.Data
{
    public static class DbRepositoryExtensions
    {
        public static void LoadRelated<TEntity>(this IDbRepository<DbContext> dbRepository, TEntity entity,
            params string[] relationships) where TEntity : class
        {
            if (entity == null || relationships == null || relationships.Length == 0)
            {
                return;
            }

            dbRepository.Transaction(dbContext =>
            {
                var entry = dbContext.Entry(entity);
                foreach (var relationship in relationships)
                {
                    var member = entry.Member(relationship);
                    if (member is DbCollectionEntry collection)
                    {
                        collection.Load();
                        continue;
                    }

                    var reference = member as DbReferenceEntry;
                    reference?.Load();
                }
            });
        }

        public static int TransactionWithSaveChanges(this IDbRepository<DbContext> dbRepository,
            Action<DbContext> transaction)
        {
            var result = 0;
            dbRepository.Transaction(dbContext =>
            {
                transaction(dbContext);
                result = dbContext.SaveChanges();
            });

            return result;
        }
    }
}
