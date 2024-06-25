using TipRecipe.Interfaces;
using TipRecipe.Models;

namespace TipRecipe.Services
{
    public class CachedRatingScoreService : ICachedRatingScoreService
    {
        private readonly CachingFileService _cachingFileService;
        private readonly string _cacheKey = "RATINGS";
        private readonly double _lifeTimeDuration = 15;

        public CachedRatingScoreService(CachingFileService cacheFileService)
        {
            _cachingFileService = cacheFileService ?? throw new ArgumentNullException(nameof(cacheFileService));
        }

        public async Task<Dictionary<string, Dictionary<string, CachedRating>>?> GetRatingsAsync()
        {
            return await _cachingFileService.GetAsync<Dictionary<string, Dictionary<string, CachedRating>>>(_cacheKey);
        }

        public async Task OverwriteRatingAsync(Dictionary<string, Dictionary<string, CachedRating>> ratingMap)
        {
            await _cachingFileService.SetAsync(_cacheKey, ratingMap, TimeSpan.FromMinutes(_lifeTimeDuration));
        }

        public async Task<bool> UpdateRatingsAsync(Dictionary<string, Dictionary<string, CachedRating>> ratingMap)
        {
            return await _cachingFileService.UpdateAsync(_cacheKey, ratingMap);
        }

        public async Task<bool> UpdateRatingAsync(CachedRating newRating, string userID, string dishID)
        {
            Dictionary<string, Dictionary<string, CachedRating>>? ratings = await this.GetRatingsAsync();
            if(ratings is not null
                && ratings.TryGetValue(userID, out var ratingsByUser)
                && ratingsByUser.ContainsKey(dishID))
            {
                ratingsByUser[dishID] = newRating;
                return await this.UpdateRatingsAsync(ratings);
            }
            return false;
        }

    }
}
