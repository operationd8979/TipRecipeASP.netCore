namespace TipRecipe.Interfaces
{
    public interface IDataRepository<T,U>
    {
        IEnumerable<T> GetAll();
        T GetByID(U id);
        T Add(T entity);
        T Update(T entity);
    }
}
