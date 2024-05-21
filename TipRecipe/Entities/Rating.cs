using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TipRecipe.Entities
{
    public class Rating
    {
        [Required]
        public string UserID { get; set; } = string.Empty;
        public string DishID { get; set; } = string.Empty;
        [ForeignKey("UserID")]
        public User User { get; set; }
        [ForeignKey("DishID")]
        public Dish Dish { get; set; }
        public float RatingScore { get; set; }
        public float PreRatingScore { get; set; }
        
    }
}
