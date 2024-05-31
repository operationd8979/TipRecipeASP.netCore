namespace TipRecipe.Models
{
    public class CachedRating
    {
        public float RatingScore { get; set; }
        public bool IsRated { get; set; } = false;
        public bool IsPreRated { get; set; } = false;

        public CachedRating()
        {
        }

        public CachedRating(float ratingScore, bool isRated)
        {
            RatingScore = ratingScore;
            IsRated = isRated;
        }

        public CachedRating(float ratingScore, bool isRated, bool isPreRated)
        {
            RatingScore = ratingScore;
            IsRated = isRated;
            IsPreRated = isPreRated;
        }
    }
}
