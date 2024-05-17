using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TipRecipe.Entities
{
    public class DetailIngredientDish
    {
        [Required]
        public string DishID;
        [Required]
        public int IngredientID;

        [ForeignKey("DishID")]
        public Dish Dish { get; set; }

        [ForeignKey("IngredientID")]
        public Ingredient Ingredient { get; set; }

        [Required]
        public int Amount { get; set; }

        [Required]
        [MaxLength(50)]
        public string Unit { get; set; }

    }
}
