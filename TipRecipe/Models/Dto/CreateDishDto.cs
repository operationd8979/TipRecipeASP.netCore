using System.ComponentModel.DataAnnotations;

namespace TipRecipe.Models.Dto
{
    public class CreateDishDto
    {
        [MaxLength(255)]
        public string DishName { get; set; }

        [MaxLength(255)]
        public string Summary { get; set; }

        [MaxLength(255)]
        public string UrlPhoto { get; set; }

        [Range(0, 10)]
        public float AvgRating { get; set; }

        public IList<DetailIngredientDishDto> DetailIngredientDishes { get; set; }

        public ICollection<DetailTypeDishDto> DetailTypeDishes { get; set; }


        public RecipeDto Recipe { get; set; }

        public CreateDishDto()
        {
        }


        public CreateDishDto(string dishName, string summary, string url, float avgRating)
        {
            DishName = dishName;
            Summary = summary;
            UrlPhoto = url;
            AvgRating = avgRating;
        }

    }
}
