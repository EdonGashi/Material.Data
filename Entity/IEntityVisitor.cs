namespace Material.Data.Entity
{
    public interface IEntityVisitor
    {
        void Visit<TEntity>(TEntity entity, GraphState state) where TEntity : class;
    }
}
