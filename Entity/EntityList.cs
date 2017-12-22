using System.Collections.Generic;
using System.Data.Entity;

namespace Material.Data.Entity
{
    internal class EntityList : Entity
    {
        private readonly IEnumerable<Entity> entities;

        public EntityList(IEnumerable<Entity> entities) : base(null)
        {
            this.entities = entities ?? new Entity[0];
        }

        public override void Accept(IEntityVisitor visitor)
        {
            foreach (var entity in entities)
            {
                entity.Accept(visitor);
            }
        }

        public override void AttachToContext(DbContext dbContext, HashSet<object> attachedEntities)
        {
            foreach (var entity in entities)
            {
                entity?.AttachToContext(dbContext, attachedEntities);
            }
        }

        public override void ChangesCommited()
        {
            foreach (var entity in entities)
            {
                entity?.ChangesCommited();
            }
        }
    }
}
