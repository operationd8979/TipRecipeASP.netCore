using TipRecipe.Entities;

namespace TipRecipe.Interfaces
{
    public interface IIngredientRepository
    {
        Task<IEnumerable<Ingredient>> GetAllAsync();
        int SaveChanges();
        Task<int> SaveChangesAsync();
    }
}
