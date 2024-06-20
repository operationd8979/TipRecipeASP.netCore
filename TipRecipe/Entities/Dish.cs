using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using TipRecipe.Helper;
using TipRecipe.Models.Dto;
using TipRecipe.Validations;

namespace TipRecipe.Entities
{
    public class Dish
    {
        [Key]
        [MaxLength(60)]
        public string DishID { get; set; }

        [Required]
        [MaxLength(255)]
        public string? DishName { get; set; }

        [Required]
        [MaxLength(255)]
        public string? Summary { get; set; }

        [AllowNull]
        [MaxLength(255)]
        public string? UrlPhoto { get; set; }

        [AllowNull]
        [Required]
        public bool IsDeleted { get; set; }

        [LowerThanTen]
        [GreaterThanZero]
        public float AvgRating { get; set; }

        [NotMapped]
        public float RatingScore { get; set; }
        [NotMapped]
        public bool isRated { get; set; } = true;

        public ICollection<Rating> Ratings { get; set; } = new List<Rating>();

        public ICollection<DetailIngredientDish> DetailIngredientDishes { get; set; } = new List<DetailIngredientDish>();

        public ICollection<DetailTypeDish> DetailTypeDishes { get; set; } = new List<DetailTypeDish>();

        public Recipe? Recipe { get; set; }

        public Dish()
        {
            this.DishID = IdGenerator.GenerateDishID();
            this.AvgRating = 0f;
            this.IsDeleted = false;
        }

        
        public override string ToString()
        {
            return $"[DishID]={this.DishID},[DishName]={this.DishName},[Summary]={this.Summary},[UrlPhoto]={this.UrlPhoto},[AvgRating]={this.AvgRating}";
        }


        public override bool Equals(object? obj)
        {
            var item = obj as Dish;
            if (item is null) return false;
            return this.DishID == (item.DishID)
                && this.DishName == (item.DishName)
                && this.Summary == (item.Summary)
                && this.UrlPhoto == (item.UrlPhoto);
        }


        public override int GetHashCode()
        {
            return HashCode.Combine(this.DishID,this.DishName, this.Summary, this.UrlPhoto);
        }


    }
}
