using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Material.Data.Entity
{
    public abstract class Entity
    {
        public static Entity List(params Entity[] entities) => new EntityList(entities);

        public static Entity<TEntity> Unchanged<TEntity>(TEntity entity) where TEntity : class
        {
            return new Entity<TEntity>(entity, GraphState.Unchanged);
        }

        public static EntityCollection<TEntity> Unchanged<TEntity>(IEnumerable<TEntity> entities) where TEntity : class
        {
            return new EntityCollection<TEntity>(entities);
        }

        public static Entity<TEntity> Added<TEntity>(TEntity entity) where TEntity : class
        {
            return new Entity<TEntity>(entity, GraphState.Added);
        }

        public static EntityContainer<TEntity> AddedGraph<TEntity>(TEntity entity) where TEntity : class
        {
            return new AddedEntityGraph<TEntity>(entity);
        }

        public static EntityContainer<TEntity> AddedGraphList<TEntity>(IEnumerable<TEntity> entities) where TEntity : class
        {
            return new AddedEntityGraph<TEntity>(entities);
        }

        public static Entity<TEntity> Modified<TEntity>(TEntity entity) where TEntity : class
        {
            return new Entity<TEntity>(entity, GraphState.Modified);
        }

        public static Entity<TEntity> Deleted<TEntity>(TEntity entity) where TEntity : class
        {
            return new Entity<TEntity>(entity, GraphState.Deleted);
        }

        internal static Dictionary<Type, object> KeyMapDictionary = new Dictionary<Type, object>();

        internal static Dictionary<TypeMap, object> ChildrenMapDictionary = new Dictionary<TypeMap, object>();

        protected Entity(Type entityType)
        {
            EntityType = entityType;
        }

        public Type EntityType { get; }

        public static void MapKeys<TEntity>(Func<TEntity, object[]> mapper)
        {
            if (mapper == null)
            {
                throw new ArgumentNullException(nameof(mapper));
            }

            KeyMapDictionary.Add(typeof(TEntity), mapper);
        }

        public static void MapChildren<TEntity, TChildEntity>(Func<TEntity, ICollection<TChildEntity>> mapper)
        {
            if (mapper == null)
            {
                throw new ArgumentNullException(nameof(mapper));
            }

            ChildrenMapDictionary.Add(TypeMap.Create<TEntity, TChildEntity>(), mapper);
        }

        public abstract void Accept(IEntityVisitor visitor);

        internal void Commit(DbContext dbContext)
        {
            var attachedEntities = new HashSet<object>();
            AttachToContext(dbContext, attachedEntities);
            var entries = dbContext.ChangeTracker.Entries().ToList();
            foreach (var entry in entries)
            {
                if (!attachedEntities.Contains(entry.Entity))
                {
                    entry.State = EntityState.Unchanged;
                }
            }

            dbContext.SaveChanges();
            ChangesCommited();
        }

        public abstract void AttachToContext(DbContext dbContext, HashSet<object> attachedEntities);

        public abstract void ChangesCommited();

        protected static EntityState Convert(GraphState state)
        {
            switch (state)
            {
                case GraphState.Added:
                    return EntityState.Added;
                case GraphState.Deleted:
                    return EntityState.Deleted;
                case GraphState.Modified:
                    return EntityState.Modified;
                case GraphState.Unchanged:
                    return EntityState.Unchanged;
                default:
                    return EntityState.Detached;
            }
        }
    }
}
