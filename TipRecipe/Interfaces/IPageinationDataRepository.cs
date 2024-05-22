namespace TipRecipe.Interfaces
{
    public interface IPageinationDataRepository<T> : IDataRepository<T>
    {
        IEnumerable<T> GetAll(int offset, int limit);
    }
}
