using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Material.Data.Entity
{
    public class Entity<TEntity> : EntityContainer<TEntity> where TEntity : class
    {
        public static void MapChildren<TChildEntity>(Func<TEntity, ICollection<TChildEntity>> mapper) where TChildEntity : class
        {
            Entity.MapChildren(mapper);
        }

        public static void MapKey(Func<TEntity, object> mapper)
        {
            Entity.MapKeys((TEntity entity) => new[] { mapper(entity) });
        }

        public static void MapKeys(Func<TEntity, object[]> mapper)
        {
            Entity.MapKeys(mapper);
        }

        private Dictionary<Type, Entity> children;

        public Entity(TEntity instance, GraphState state)
        {
            Instance = instance ?? throw new ArgumentNullException(nameof(instance));
            State = state;
        }

        public GraphState State { get; set; }

        public TEntity Instance { get; }

        public EntityCollection<TChildEntity> Children<TChildEntity>() where TChildEntity : class
        {
            if (children == null)
            {
                children = new Dictionary<Type, Entity>();
            }

            var type = typeof(TChildEntity);
            if (children.TryGetValue(type, out var collectionBase))
            {
                return (EntityCollection<TChildEntity>)collectionBase;
            }

            var collection = new EntityCollection<TChildEntity>();
            children[type] = collection;
            return collection;
        }

        public Entity<TEntity> AttachChildren<TChildEntity>() where TChildEntity : class
        {
            var collection = GetChildren<TChildEntity>();
            Children<TChildEntity>().TryAttach(collection);
            return this;
        }

        public Entity<TEntity> AddChildren<TChildEntity>() where TChildEntity : class
        {
            var collection = GetChildren<TChildEntity>();
            var entityCollection = Children<TChildEntity>();
            foreach (var child in collection)
            {
                entityCollection.Add(child);
            }

            return this;
        }

        public Entity<TEntity> UpdateChildren<TChildEntity>() where TChildEntity : class
        {
            var collection = GetChildren<TChildEntity>();
            var entityCollection = Children<TChildEntity>();
            foreach (var child in collection)
            {
                entityCollection.MarkModifiedIfUnchanged(child);
            }

            return this;
        }

        public Entity<TEntity> DeleteChildren<TChildEntity>() where TChildEntity : class
        {
            var collection = GetChildren<TChildEntity>();
            collection.Clear();
            Children<TChildEntity>().DeleteAll();
            return this;
        }

        public Entity<TEntity> Add<TChildEntity>(TChildEntity childEntity) where TChildEntity : class
        {
            var collection = GetChildren<TChildEntity>();
            collection.Add(childEntity);
            Children<TChildEntity>().Add(childEntity);
            return this;
        }

        public Entity<TEntity> Delete<TChildEntity>(TChildEntity childEntity) where TChildEntity : class
        {
            if (childEntity == null)
            {
                return this;
            }

            var collection = GetChildren<TChildEntity>();
            collection.Remove(childEntity);
            Children<TChildEntity>().Delete(childEntity);
            return this;
        }

        public Entity<TEntity> Delete<TChildEntity>(IEnumerable<TChildEntity> childEntities) where TChildEntity : class
        {
            if (childEntities == null)
            {
                return this;
            }

            var collection = GetChildren<TChildEntity>();
            var children = Children<TChildEntity>();
            var list = childEntities.ToList();
            foreach (var child in list)
            {
                collection.Remove(child);
                children.Delete(child);
            }

            return this;
        }

        public Entity<TEntity> MarkModifiedIfUnchanged()
        {
            if (State == GraphState.Unchanged)
            {
                State = GraphState.Modified;
            }

            return this;
        }

        public static implicit operator TEntity(Entity<TEntity> entity) => entity.Instance;

        private ICollection<TChildEntity> GetChildren<TChildEntity>() where TChildEntity : class
        {
            if (ChildrenMapDictionary.TryGetValue(TypeMap.Create<TEntity, TChildEntity>(), out var func))
            {
                return ((Func<TEntity, ICollection<TChildEntity>>)func)(Instance);
            }

            throw new InvalidOperationException(
                $"No mapping between parent type {typeof(TEntity).FullName} and child type {typeof(TChildEntity).FullName} has been established.");
        }

        public override void Accept(IEntityVisitor visitor)
        {
            visitor.Visit(Instance, State);
            if (children == null)
            {
                return;
            }

            foreach (var container in children.Values)
            {
                container.Accept(visitor);
            }
        }

        public override void AttachToContext(DbContext dbContext, HashSet<object> attachedEntities)
        {
            attachedEntities.Add(Instance);
            var entry = dbContext.Entry(Instance);
            entry.State = EntityState.Added;
            entry.State = Convert(State);

            if (children == null)
            {
                return;
            }

            foreach (var container in children.Values)
            {
                container.AttachToContext(dbContext, attachedEntities);
            }
        }

        public override void ChangesCommited()
        {
            State = GraphState.Unchanged;
            if (children == null)
            {
                return;
            }

            foreach (var container in children.Values)
            {
                container.ChangesCommited();
            }
        }
    }
}
