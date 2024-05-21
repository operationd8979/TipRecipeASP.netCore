using System.ComponentModel.DataAnnotations;
using TipRecipe.Entities;
using TipRecipe.Validations;

namespace TipRecipe.Models.Dto
{
    public class DishDto
    {

        [MaxLength(60)]
        public string DishID { get; set; }

        [MaxLength(255)]
        public string DishName { get; set; }

        [MaxLength(255)]
        public string Summary { get; set; }

        [MaxLength(255)]
        public string UrlPhoto { get; set; }

        [GreaterThanZero]
        [Range(0, 10)]
        public float AvgRating { get; set; }

        public IList<DetailIngredientDishDto> DetailIngredientDishes { get; set; }

        public ICollection<DetailTypeDishDto> DetailTypeDishes { get; set; }


        public RecipeDto Recipe { get; set; }

        public DishDto()
        {
        }


        public DishDto(string dishID, string dishName, string summary, string url, float avgRating)
        {
            DishID = dishID;
            DishName = dishName;
            Summary = summary;
            UrlPhoto = url;
            AvgRating = avgRating;
        }


        public override string ToString()
        {
            return $"[DishID]={this.DishID},[DishName]={this.DishName},[Summary]={this.Summary},[UrlPhoto]={this.UrlPhoto},[AvgRating]={this.AvgRating}";
        }

    }
}
