using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using TipRecipe.Entities;

namespace TipRecipe.Models.Dto
{
    public class RecipeDto
    {
        [Required]
        public string? Content { get; set; }
    }
}
