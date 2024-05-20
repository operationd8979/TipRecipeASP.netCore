


using System.ComponentModel.DataAnnotations;
using TipRecipe.Validations;

namespace TipRecipe.Models.Dto
{
    public class DetailIngredientDishDto
    {
        public IngredientDto Ingredient { get; set; }

        [GreaterThanZero]
        public int Amount { get; set; }

        public string Unit { get; set; }
    }
}
