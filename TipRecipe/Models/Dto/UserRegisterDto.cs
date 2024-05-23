using System.ComponentModel.DataAnnotations;

namespace TipRecipe.Models.Dto
{
    public record UserRegisterDto([MaxLength(60)][EmailAddress]string Email, [MaxLength(255)]string UserName, [MinLength(8)][MaxLength(255)]string Password);
}
