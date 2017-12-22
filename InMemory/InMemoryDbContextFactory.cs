using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace Material.Data.InMemory
{
    public class InMemoryDbContextFactory : IDbContextFactory<DbContext>
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

        public DbContext Create()
        {
            var context = new InMemoryDbContext();
            foreach (var dbSet in dbSets)
            {
                context.RegisterDbSet(dbSet.Key, dbSet.Value);
            }

            return context;
        }
    }
}
