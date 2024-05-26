using Microsoft.EntityFrameworkCore;
using TipRecipe.Entities;
using TipRecipe.Models;

namespace TipRecipe.Interfaces
{
    public interface IDishRepository : IDataRepository<Dish>
    {
        Task<IEnumerable<Dish>> GetWithFilterAsync(string query, int[] ingredients, int[] types, int offset, int limit, string orderBy );

        Task<IEnumerable<UserDishRating>> GetUserDishRatingsAsync();

        Task UpdateAverageScoreDishes();

        Task<Rating?> GetRatingDishAsync(string dishID, string userID);

        void AddRating(Rating rating);

        Task<IEnumerable<Dish>> GetDishsByListID(List<string> dishIDs);


        Task<float?> GetRatingOfDish(string dishID, string userID);

    }
}
