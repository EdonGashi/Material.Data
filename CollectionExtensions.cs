using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Material.Data
{
    public static class CollectionExtensions
    {
        public static bool Replace<TEntity>(this ICollection<TEntity> collection, TEntity oldEntity, TEntity newEntity) where TEntity : class
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (collection is IList<TEntity> list)
            {
                for (var i = 0; i < list.Count; i++)
                {
                    if (ReferenceEquals(oldEntity, list[i]))
                    {
                        list[i] = newEntity;
                        return true;
                    }
                }

                return false;
            }

            if (collection.Remove(oldEntity))
            {
                collection.Add(newEntity);
                return true;
            }

            return false;
        }

        public static bool TryAdd<TEntity>(this ICollection<TEntity> collection, TEntity entity)
        {
            if (collection.Contains(entity))
            {
                return false;
            }

            collection.Add(entity);
            return true;
        }

        public static bool IsNullOrEmpty(this ICollection collection) => collection == null || collection.Count == 0;

        public static ObservableCollection<TEntity> ToObservable<TEntity>(this IEnumerable<TEntity> enumerable)
        {
            return new ObservableCollection<TEntity>(enumerable);
        }
    }
}
