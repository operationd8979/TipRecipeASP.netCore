namespace TipRecipe.Models
{
    public class UserDishRating
    {
        public string UserID { get; set; }
        public string DishID { get; set; }
        public float RatingScore { get; set; }
    }
}
