using System.ComponentModel.DataAnnotations;

namespace TipRecipe.Models
{
    public class UserDishRating
    {
        [Required]
        public string? UserID { get; set; }
        [Required]
        public string? DishID { get; set; }
        public float RatingScore { get; set; }
    }
}
