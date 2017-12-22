using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;

namespace Material.Data.InMemory
{
    public class InMemoryDbSet<TEntity> : DbSet<TEntity>, IQueryable, IEnumerable<TEntity>, IDbAsyncEnumerable<TEntity>
        where TEntity : class
    {
        private static int IdCounter = int.MaxValue >> 1;

        private readonly ObservableCollection<TEntity> data;
        private readonly IQueryable query;

        public InMemoryDbSet() : this(null)
        {
        }

        public InMemoryDbSet(IEnumerable<TEntity> initialData)
        {
            data = initialData != null
                ? new ObservableCollection<TEntity>(initialData)
                : new ObservableCollection<TEntity>();
            query = data.AsQueryable();
        }

        public override ObservableCollection<TEntity> Local => data;

        IDbAsyncEnumerator<TEntity> IDbAsyncEnumerable<TEntity>.GetAsyncEnumerator()
        {
            return new AsyncEnumerator<TEntity>(data.GetEnumerator());
        }

        IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator()
        {
            return data.GetEnumerator();
        }

        public override TEntity Find(params object[] keyValues)
        {
            if (keyValues == null || keyValues.Length == 0)
            {
                throw new ArgumentException("Invalid key values provided.");
            }

            object mapper;
            if (Entity.Entity.KeyMapDictionary.TryGetValue(typeof(TEntity), out mapper))
            {
                var keyOf = (Func<TEntity, object[]>)mapper;
                return data
                    .FirstOrDefault(entity => keyValues
                    .SequenceEqual(keyOf(entity)));
            }

            throw new InvalidOperationException($"Entity of type {typeof(TEntity).FullName} has no registered key mapper.");
        }

        Type IQueryable.ElementType => query.ElementType;

        Expression IQueryable.Expression => query.Expression;

        IQueryProvider IQueryable.Provider => new AsyncQueryProvider<TEntity>(query.Provider);

        IEnumerator IEnumerable.GetEnumerator()
        {
            return data.GetEnumerator();
        }

        public override TEntity Add(TEntity item)
        {
            data.Add(item);
            try
            {
                dynamic d = item;
                d.Id = IdCounter;
                IdCounter++;
            }
            catch
            {
                // ignored
            }

            return item;
        }

        public override TEntity Remove(TEntity item)
        {
            data.Remove(item);
            return item;
        }

        public override TEntity Attach(TEntity item)
        {
            data.Add(item);
            return item;
        }

        public override TEntity Create()
        {
            return Activator.CreateInstance<TEntity>();
        }

        public override TDerivedEntity Create<TDerivedEntity>()
        {
            return Activator.CreateInstance<TDerivedEntity>();
        }
    }
}
