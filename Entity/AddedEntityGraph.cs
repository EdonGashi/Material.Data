using System.Collections.Generic;
using System.Data.Entity;

namespace Material.Data.Entity
{
    internal class AddedEntityGraph<TEntity> : EntityContainer<TEntity> where TEntity : class
    {
        private readonly IEnumerable<TEntity> entities;

        public AddedEntityGraph(TEntity instance) : this(new[] { instance })
        {
        }

        public AddedEntityGraph(IEnumerable<TEntity> entities)
        {
            this.entities = entities ?? new TEntity[0];
        }

        public override void Accept(IEntityVisitor visitor)
        {
            foreach (var entity in entities)
            {
                visitor.Visit(entity, GraphState.Added);
            }
        }

        public override void AttachToContext(DbContext dbContext, HashSet<object> attachedEntities)
        {
            foreach (var entity in entities)
            {
                if (entity == null)
                {
                    continue;
                }

                dbContext.Entry(entity).State = EntityState.Added;
            }

            foreach (var entry in dbContext.ChangeTracker.Entries())
            {
                attachedEntities.Add(entry.Entity);
            }
        }

        public override void ChangesCommited()
        {
        }
    }
}