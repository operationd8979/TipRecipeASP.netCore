using System.ComponentModel.DataAnnotations;

namespace TipRecipe.Models.Dto
{
    public class TypeDishDto
    {
        public int TypeID { get; set; }
        [Required]
        public string? TypeName { get; set; }
    }
}
