using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TipRecipe.Entities
{
    public class TypeDish
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TypeID { get; set; }

        [Required]
        [MaxLength(255)]
        public string? TypeName { get; set; }
    }
}
