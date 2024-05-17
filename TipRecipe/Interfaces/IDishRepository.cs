using TipRecipe.Entities;

namespace TipRecipe.Interfaces
{
    public interface IDishRepository
    {
        Task<IEnumerable<Dish>> GetAllAsync();
        Task<IEnumerable<Dish>> GetWithFilterAsync(string query, int[] ingredients, int[] types, int offset, int limit, string orderBy );
        Task<Dish> GetByIDAsync(string dishID);
        Task AddAsync(Dish newDish);
        Task UpdateAsync(Dish updateDish);

        int SaveChanges();

        Task<int> SaveChangesAsync();

    }
}
