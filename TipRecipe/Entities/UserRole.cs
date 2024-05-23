using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TipRecipe.Entities
{
    public class UserRole
    {
        [ForeignKey("UserID")]
        public string UserID { get; set; }

        public User? User { get; set; }

        [Required]
        [MaxLength(60)]
        public RoleType Role { get; set; }

        public UserRole(RoleType role)
        {
            this.UserID = string.Empty;
            this.Role = role;
        }
    }
}
