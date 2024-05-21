using System.ComponentModel.DataAnnotations;

namespace TipRecipe.Entities
{
    public class User
    {
        [Key]
        public string UserID { get; set; }
    }
}
