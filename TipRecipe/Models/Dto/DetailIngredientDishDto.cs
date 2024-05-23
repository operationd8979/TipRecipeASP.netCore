


using System.ComponentModel.DataAnnotations;
using TipRecipe.Validations;

namespace TipRecipe.Models.Dto
{
    public class DetailIngredientDishDto
    {
        [Required]
        public IngredientDto? Ingredient { get; set; }

        [GreaterThanZero]
        public int Amount { get; set; }

        [Required]
        public string? Unit { get; set; }
    }
}
