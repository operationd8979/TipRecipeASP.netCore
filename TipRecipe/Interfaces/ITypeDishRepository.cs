using TipRecipe.Entities;

namespace TipRecipe.Interfaces
{
    public interface ITypeDishRepository
    {
        Task<IEnumerable<TypeDish>> GetAllAsync();
        int SaveChanges();
        Task<int> SaveChangesAsync();
    }
}
