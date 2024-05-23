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
        public IList<DetailIngredientDishDto>? DetailIngredientDishes { get; set; }

        [Required]
        public ICollection<DetailTypeDishDto>? DetailTypeDishes { get; set; }

        [Required]
        public RecipeDto? Recipe { get; set; }

        public CreateDishDto()
        {
        }


    }
}
