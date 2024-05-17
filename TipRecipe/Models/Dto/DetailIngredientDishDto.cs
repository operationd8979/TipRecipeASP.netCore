using TipRecipe.Entities;

namespace TipRecipe.Models.Dto
{
    public class DetailIngredientDishDto
    {
        public IngredientDto Ingredient { get; set; }
        public int Amount { get; set; }
        public string Unit { get; set; }
    }
}
