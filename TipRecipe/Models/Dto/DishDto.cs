using System.ComponentModel.DataAnnotations;
using TipRecipe.Entities;

namespace TipRecipe.Models.Dto
{
    public class DishDto
    {

        [MaxLength(40)]
        public string DishID { get; set; }

        [MaxLength(255)]
        public string DishName { get; set; }

        [MaxLength(255)]
        public string Summary { get; set; }

        [MaxLength(255)]
        public string UrlPhoto { get; set; }

        public DishDto()
        {
        }


        public DishDto(string dishID, string dishName, string summary, string url)
        {
            DishID = dishID;
            DishName = dishName;
            Summary = summary;
            UrlPhoto = url;
        }

        public DishDto(Dish dish)
        {
            DishID = dish.DishID;
            DishName = dish.DishName;
            Summary = dish.Summary;
            UrlPhoto = dish.UrlPhoto;
        }

        public override string ToString()
        {
            return $"[DishID]={this.DishID},[DishName]={this.DishName},[Summary]={this.Summary},[UrlPhoto]={this.UrlPhoto}";
        }

    }
}
