using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace Material.Data
{
    public class DbRepository<TDbContext> : IDbRepository<TDbContext> where TDbContext : DbContext
    {
        private readonly IDbContextFactory<TDbContext> dbContextFactory;

        public DbRepository(IDbContextFactory<TDbContext> dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory;
        }

        public void Transaction(Action<TDbContext> transaction)
        {
            using (var context = dbContextFactory.Create())
            {
                transaction(context);
            }
        }

        public TResult Query<TResult>(Func<TDbContext, TResult> query)
        {
            using (var context = dbContextFactory.Create())
            {
                context.Configuration.AutoDetectChangesEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                return query(context);
            }
        }
    }
}
