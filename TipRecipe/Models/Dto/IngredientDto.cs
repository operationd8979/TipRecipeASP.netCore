using System.ComponentModel.DataAnnotations;

namespace TipRecipe.Models.Dto
{
    public class IngredientDto
    {
        public int IngredientId { get; set; }

        [Required]
        public string? IngredientName { get; set; }
    }
}
