using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using TipRecipe.Validations;

namespace TipRecipe.Entities
{
    public class Rating
    {
        [Required]
        public string UserID { get; set; } = string.Empty;
        public string DishID { get; set; } = string.Empty;
        [ForeignKey("UserID")]
        public User? User { get; set; }
        [ForeignKey("DishID")]
        public Dish? Dish { get; set; }
        [LowerThanTen]
        [GreaterThanZero]
        public float RatingScore { get; set; }
        [LowerThanTen]
        [GreaterThanZero]
        public float PreRatingScore { get; set; }
        [Required]
        public DateTime RatedAt { get; set; }

        public Rating()
        {
        }

        public Rating(string userID, string dishID, float ratingScore, float preRatingScore, DateTime ratedAt)
        {
            UserID = userID;
            DishID = dishID;
            RatingScore = ratingScore;
            PreRatingScore = preRatingScore;
            RatedAt = ratedAt;
        }
    }
}
