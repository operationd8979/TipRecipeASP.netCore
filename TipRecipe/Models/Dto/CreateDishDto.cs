using System.ComponentModel.DataAnnotations;

namespace TipRecipe.Models.Dto
{
    public class CreateDishDto
    {
        [Required]
        [MaxLength(255)]
        public string? DishName { get; set; }

        [Required]
        [MaxLength(255)]
        public string? Summary { get; set; }

        [MaxLength(255)]
        public string? UrlPhoto { get; set; } = string.Empty;

        [Required]
        public ICollection<DetailIngredientDishDto>? DetailIngredientDishes { get; set; }

        [Required]
        public ICollection<DetailTypeDishDto>? DetailTypeDishes { get; set; }

        //[Required]
        public RecipeDto? Recipe { get; set; }

        public CreateDishDto()
        {
        }

        public CreateDishDto(string? dishName, string? summary, string? urlPhoto, ICollection<DetailIngredientDishDto>? detailIngredientDishes, ICollection<DetailTypeDishDto>? detailTypeDishes, RecipeDto? recipe)
        {
            DishName = dishName;
            Summary = summary;
            UrlPhoto = urlPhoto;
            DetailIngredientDishes = detailIngredientDishes;
            DetailTypeDishes = detailTypeDishes;
            Recipe = recipe;
        }


    }
}
