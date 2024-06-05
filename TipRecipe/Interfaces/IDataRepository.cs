using TipRecipe.Entities;
using TipRecipe.Models;

namespace TipRecipe.Interfaces
{
    public interface IDataRepository<T>
    {
        Task<IEnumerable<T>> GetAllAsync();
        IAsyncEnumerable<T> GetAllEnumerableAsync();
        Task<T?> GetByIDAsync(string id);
        void Add(T newObject);
        void Update(T updateObject);
        bool SaveChanges();
        Task<bool> SaveChangesAsync();

    }
}
