using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TipRecipe.Entities
{
    public class DetailTypeDish
    {
        [Required]
        public string DishID { get; set; } = string.Empty;
        [Required]
        public int TypeID { get; set; }


        [ForeignKey("DishID")]
        public Dish Dish { get; set; }
        [ForeignKey("TypeID")]
        public TypeDish Type { get; set; }

        public DetailTypeDish(int typeID)
        {
            this.TypeID = typeID;
        }
    }
}
