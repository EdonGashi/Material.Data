using System;
using System.Data.Entity;

namespace Material.Data
{
    public interface IDbRepository<out TDbContext> where TDbContext : DbContext
    {
        void Transaction(Action<TDbContext> transaction);

        TResult Query<TResult>(Func<TDbContext, TResult> query);
    }
}
