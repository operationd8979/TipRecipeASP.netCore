using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TipRecipe.Entities
{
    public class Ingredient
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int IngredientID { get; set; }
        [Required]
        [MaxLength(255)]
        public string IngredientName { get; set; }

        public Ingredient()
        {
            this.IngredientName = "";
        }

        public Ingredient(int ingredientID, string ingredientName)
        {
            this.IngredientID = ingredientID;
            this.IngredientName = ingredientName;
        }


    }
}
