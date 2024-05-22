using TipRecipe.Entities;
using TipRecipe.Models;

namespace TipRecipe.Interfaces
{
    public interface IDishRepository : IDataRepository<Dish>
    {
        Task<IEnumerable<Dish>> GetWithFilterAsync(string query, int[] ingredients, int[] types, int offset, int limit, string orderBy );

        Task<IEnumerable<UserDishRating>> GetUserDishRatingsAsync();

        Task UpdateAverageScoreDishes();

    }
}
