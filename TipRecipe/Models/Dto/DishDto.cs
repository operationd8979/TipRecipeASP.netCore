using System.ComponentModel.DataAnnotations;
using TipRecipe.Entities;
using TipRecipe.Validations;

namespace TipRecipe.Models.Dto
{
    public class DishDto
    {

        [MaxLength(60)]
        public string? DishID { get; set; }

        [MaxLength(255)]
        public string? DishName { get; set; }

        [MaxLength(255)]
        public string? Summary { get; set; }

        [MaxLength(255)]
        public string? UrlPhoto { get; set; }

        [LowerThanTen]
        [GreaterThanZero]
        public float AvgRating { get; set; }

        [LowerThanTen]
        [GreaterThanZero]
        public float? RatingScore { get; set; }
        public bool IsRated { get; set; }
        public bool IsDeleted { get; set; }

        public IList<DetailIngredientDishDto> DetailIngredientDishes { get; set; } = new List<DetailIngredientDishDto>();

        public ICollection<DetailTypeDishDto> DetailTypeDishes { get; set; } = new List<DetailTypeDishDto>();


        public RecipeDto? Recipe { get; set; }

        public DishDto()
        {
        }


        public override string ToString()
        {
            return $"[DishID]={this.DishID},[DishName]={this.DishName},[Summary]={this.Summary},[UrlPhoto]={this.UrlPhoto},[AvgRating]={this.AvgRating}";
        }

    }
}
