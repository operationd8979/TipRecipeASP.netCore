using TipRecipe.Models;

namespace TipRecipe.Interfaces
{
    public interface ICachedRatingScoreService : ICachedObjectService<Dictionary<string, Dictionary<string, CachedRating>>>
    {
        public Task<bool> UpdateRatingAsync(CachedRating newRating, string userID, string dishID);
    }
}
