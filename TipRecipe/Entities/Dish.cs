using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using TipRecipe.Models.Dto;

namespace TipRecipe.Entities
{
    public class Dish
    {
        [Key]
        [Required]
        [MaxLength(40)]
        public string DishID { get; set; }

        [Required]
        [MaxLength(255)]
        public string DishName { get; set; }

        [Required]
        [MaxLength(255)]
        public string Summary { get; set; }

        [MaxLength(255)]
        public string UrlPhoto { get; set; }

        public Dish()
        {
        }

        public Dish(string dishID, string dishName, string summary, string urlPhoto)
        {
            this.DishID = dishID;
            this.DishName = dishName;
            this.Summary = summary;
            this.UrlPhoto = urlPhoto;
        }

        public Dish(DishDto dishDto)
        {
            this.DishID = dishDto.DishID;
            this.DishName = dishDto.DishName;
            this.Summary = dishDto.Summary;
            this.UrlPhoto = dishDto.UrlPhoto;
        }
        

        public override string ToString()
        {
            return $"[DishID]={this.DishID},[DishName]={this.DishName},[Summary]={this.Summary},[UrlPhoto]={this.UrlPhoto}";
        }


        public override bool Equals(object? other)
        {
            var item = other as Dish;
            if (item == null) return false;
            return this.DishID.Equals(item.DishID)
                && this.DishName.Equals(item.DishName)
                && this.Summary.Equals(item.Summary)
                && this.UrlPhoto.Equals(item.UrlPhoto);
        }


        public override int GetHashCode()
        {
            return HashCode.Combine(this.DishID,this.DishName, this.Summary, this.UrlPhoto);
        }

        public static bool operator ==(Dish a, Dish b)
        {
            if (a is null)
                return b is null;

            return a.Equals(b);
        }

        public static bool operator !=(Dish b1, Dish b2)
        {
            return !(b1 == b2);
        }


    }
}
