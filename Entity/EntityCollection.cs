using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;

namespace Material.Data.Entity
{
    public class EntityCollection<TEntity> : EntityContainer<TEntity>, IEnumerable<TEntity> where TEntity : class
    {
        private readonly Dictionary<TEntity, Entity<TEntity>> entities;
        private readonly ObservableCollection<TEntity> observableCollection;

        public EntityCollection()
            : this(null, true)
        {
        }

        public EntityCollection(bool observeChanges)
            : this(null, observeChanges)
        {
        }

        public EntityCollection(IEnumerable<TEntity> instances)
            : this(instances, true)
        {
        }

        public EntityCollection(IEnumerable<TEntity> instances, bool observeChanges)
        {
            entities = new Dictionary<TEntity, Entity<TEntity>>();
            if (observeChanges)
            {
                observableCollection = new ObservableCollection<TEntity>();
                ObservableCollection = new ReadOnlyObservableCollection<TEntity>(observableCollection);
            }

            Attach(instances);
        }

        public ReadOnlyObservableCollection<TEntity> ObservableCollection { get; }

        public IEnumerable<Entity<TEntity>> Entities => entities.Values.ToList();

        public IEnumerator<TEntity> GetEnumerator() => entities.Values
            .Where(entity => entity.State != GraphState.Deleted)
            .Select(entity => entity.Instance)
            .GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public EntityCollection<TEntity> Merge(Entity<TEntity> entity)
        {
            if (entities.ContainsKey(entity.Instance))
            {
                throw new InvalidOperationException("Current collection already contains this entity.");
            }

            entities[entity.Instance] = entity;
            observableCollection?.Add(entity.Instance);
            return this;
        }

        public void Attach(TEntity instance) => Entity(instance).State = GraphState.Unchanged;

        public void Attach(IEnumerable<TEntity> entityInstances)
        {
            if (entityInstances == null)
            {
                return;
            }

            foreach (var entity in entityInstances)
            {
                if (entity != null)
                {
                    Attach(entity);
                }
            }
        }

        public void TryAttach(TEntity instance) => Entity(instance);

        public void TryAttach(IEnumerable<TEntity> entityInstances)
        {
            if (entityInstances == null)
            {
                return;
            }

            foreach (var entity in entityInstances)
            {
                if (entity != null)
                {
                    TryAttach(entity);
                }
            }
        }

        public void MarkModified(TEntity instance)
        {
            Entity(instance).State = GraphState.Modified;
        }

        public void MarkModifiedOrAdd(TEntity instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            if (entities.TryGetValue(instance, out var entity))
            {
                if (entity.State != GraphState.Added)
                {
                    entity.State = GraphState.Modified;
                }
            }
            else
            {
                Add(instance);
            }
        }

        public void MarkModifiedIfUnchanged(TEntity instance)
        {
            var entity = Entity(instance);
            if (entity.State == GraphState.Unchanged)
            {
                entity.State = GraphState.Modified;
            }
        }

        public void MarkAllModifiedIfUnchanged()
        {
            foreach (var entity in entities.Values)
            {
                if (entity.State == GraphState.Unchanged)
                {
                    entity.State = GraphState.Modified;
                }
            }
        }

        public void Detach(TEntity instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            if (entities.ContainsKey(instance))
            {
                entities.Remove(instance);
                observableCollection?.Remove(instance);
            }
        }

        public Entity<TEntity> Entity(TEntity instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            Entity<TEntity> entity;
            if (entities.TryGetValue(instance, out entity))
            {
                return entity;
            }

            entities[instance] = entity = new Entity<TEntity>(instance, GraphState.Unchanged);
            observableCollection?.Add(instance);
            return entity;
        }

        public Entity<TEntity> ExistingEntity(TEntity instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            if (entities.TryGetValue(instance, out var entity))
            {
                return entity;
            }

            throw new InvalidOperationException("Entity instance does not exist in current collection.");
        }

        public Entity<TEntity> NonExistingEntity(TEntity instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            if (entities.ContainsKey(instance))
            {
                throw new InvalidOperationException("Entity instance already exists in current collection.");
            }

            var entity = new Entity<TEntity>(instance, GraphState.Unchanged);
            entities[instance] = entity;
            observableCollection?.Add(instance);
            return entity;
        }

        public void Add(TEntity instance)
        {
            NonExistingEntity(instance).State = GraphState.Added;
        }

        public void DeleteAll()
        {
            foreach (var entity in entities.Values.ToList())
            {
                Delete(entity);
            }
        }

        public void Delete(TEntity instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            if (entities.TryGetValue(instance, out var entity))
            {
                Delete(entity);
            }
        }

        private void Delete(Entity<TEntity> entity)
        {
            if (entity.State == GraphState.Added)
            {
                entities.Remove(entity.Instance);
            }

            entity.State = GraphState.Deleted;
            observableCollection?.Remove(entity.Instance);
        }

        public override void Accept(IEntityVisitor visitor)
        {
            foreach (var entity in entities.Values)
            {
                entity.Accept(visitor);
            }
        }

        public override void AttachToContext(DbContext dbContext, HashSet<object> attachedEntities)
        {
            foreach (var entity in entities.Values)
            {
                entity.AttachToContext(dbContext, attachedEntities);
            }
        }

        public override void ChangesCommited()
        {
            foreach (var pair in entities.ToList())
            {
                if (pair.Value.State == GraphState.Deleted)
                {
                    entities.Remove(pair.Key);
                    continue;
                }

                pair.Value.ChangesCommited();
            }
        }
    }
}
