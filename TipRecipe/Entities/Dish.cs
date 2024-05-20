using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using TipRecipe.Models.Dto;

namespace TipRecipe.Entities
{
    public class Dish
    {
        [Key]
        [MaxLength(60)]
        public string DishID { get; set; }

        [Required]
        [MaxLength(255)]
        public string DishName { get; set; }

        [Required]
        [MaxLength(255)]
        public string Summary { get; set; }

        [AllowNull]
        [MaxLength(255)]
        public string UrlPhoto { get; set; }

        [AllowNull]
        [Required]
        public bool IsDeleted { get; set; }

        [Required]
        public float AvgRating { get; set; }

        public ICollection<DetailIngredientDish> DetailIngredientDishes { get; set; } = new List<DetailIngredientDish>();

        public ICollection<DetailTypeDish> DetailTypeDishes { get; set; } = new List<DetailTypeDish>();

        public Recipe Recipe { get; set; }

        public Dish()
        {
            this.DishID = string.Empty;
            this.IsDeleted = false;
        }


        public Dish(string dishID, string dishName, string summary, string urlPhoto,float avgRating)
        {
            this.DishID = dishID;
            this.DishName = dishName;
            this.Summary = summary;
            this.UrlPhoto = urlPhoto;
            this.AvgRating = avgRating;
            this.IsDeleted = false;
        }

        
        public override string ToString()
        {
            return $"[DishID]={this.DishID},[DishName]={this.DishName},[Summary]={this.Summary},[UrlPhoto]={this.UrlPhoto},[AvgRating]={this.AvgRating}";
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
