using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Threading;
using System.Threading.Tasks;

namespace Material.Data.InMemory
{
    public class InMemoryDbContext : DbContext
    {
        static InMemoryDbContext()
        {
            Database.SetInitializer<InMemoryDbContext>(null);
        }

        private readonly Dictionary<Type, object> dbSets = new Dictionary<Type, object>();

        internal void RegisterDbSet(Type entityType, object dbSet)
        {
            dbSets.Add(entityType, dbSet);
        }

        public void RegisterDbSet<TEntity>(InMemoryDbSet<TEntity> dbSet) where TEntity : class
        {
            if (dbSet == null)
            {
                throw new ArgumentNullException(nameof(dbSet));
            }

            dbSets.Add(typeof(TEntity), dbSet);
        }

        public override int SaveChanges() => -1;

        public override Task<int> SaveChangesAsync() => Task.FromResult(-1);

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken)
            => SaveChangesAsync();

        public override DbSet<TEntity> Set<TEntity>()
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

        public override DbSet Set(Type entityType)
        {
            throw new NotSupportedException();
        }
    }
}
