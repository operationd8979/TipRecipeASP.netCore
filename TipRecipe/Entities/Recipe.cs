using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TipRecipe.Entities
{
    public class Recipe
    {
        [Key]
        public string DishID { get; set; }

        [ForeignKey("DishID")]
        public Dish? Dish { get; set; }

        [Column(TypeName = "nvarchar(MAX)")]
        public string? Content { get; set; }

        public Recipe()
        {
            this.DishID = string.Empty;
        }

    }
}
