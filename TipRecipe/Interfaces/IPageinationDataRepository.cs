namespace TipRecipe.Interfaces
{
    public interface IPageinationDataRepository<T,U> : IDataRepository<T,U>
    {
        IEnumerable<T> GetAll(int offset, int limit);
    }
}
