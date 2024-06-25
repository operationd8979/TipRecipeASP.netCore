using TipRecipe.Models;
using TipRecipe.Services;

namespace TipRecipe.Interfaces
{
    public interface ICachedObjectService<T>
    {
        public Task<T?> GetRatingsAsync();

        public Task OverwriteRatingAsync(T value);

        public Task<bool> UpdateRatingsAsync(T value);

    }
}
