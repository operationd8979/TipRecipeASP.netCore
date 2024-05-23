using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using TipRecipe.Helper;

namespace TipRecipe.Entities
{
    public class User
    {
        [Key]
        [MaxLength(60)]
        public string UserID { get; set; }

        [Required]
        [MaxLength(255)]
        public string? UserName { get; set; }

        [Required]
        [MaxLength(255)]
        public string? Password { get; set; }

        [Required]
        [MaxLength(255)]
        public string? Email { get; set; }

        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

        public User()
        {
            this.UserID = IdGenerator.GenerateUserID();
        }

    }
}
