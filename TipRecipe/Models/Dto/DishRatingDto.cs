using System.ComponentModel.DataAnnotations;
using TipRecipe.Validations;

namespace TipRecipe.Models.Dto
{
    public record DishRatingDto([MaxLength(60)] string DishID, [GreaterThanZero][LowerThanTen]float RatingScore);
}
