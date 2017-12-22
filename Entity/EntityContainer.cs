namespace Material.Data.Entity
{
    public abstract class EntityContainer<TEntity> : Entity where TEntity : class
    {
        protected EntityContainer() : base(typeof(TEntity))
        {
        }
    }
}
