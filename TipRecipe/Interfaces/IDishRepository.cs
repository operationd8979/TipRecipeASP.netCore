using TipRecipe.Entities;

namespace TipRecipe.Interfaces
{
    public interface IDishRepository
    {
        Task<IEnumerable<Dish>> GetAllAsync();
        Task<IEnumerable<Dish>> GetWithFilterAsync(string query, int[] ingredients, int[] types, int offset, int limit, string orderBy );
        Task<Dish?> GetByIDAsync(string dishID);
        void Add(Dish newDish);
        void Update(Dish updateDish);

        bool SaveChanges();

        Task<bool> SaveChangesAsync();

    }
}
