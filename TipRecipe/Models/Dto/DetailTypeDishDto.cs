using System.ComponentModel.DataAnnotations;

namespace TipRecipe.Models.Dto
{
    public class DetailTypeDishDto
    {
        [Required]
        public TypeDishDto? Type { get; set; }
    }
}
