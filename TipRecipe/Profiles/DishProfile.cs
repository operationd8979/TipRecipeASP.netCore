using AutoMapper;
using TipRecipe.Entities;
using TipRecipe.Models.Dto;

namespace TipRecipe.Profiles
{
    public class DishProfile : Profile
    {
        public DishProfile()
        {
            CreateMap<Dish, DishDto>();
            CreateMap<DishDto, Dish>();
            CreateMap<Ingredient, IngredientDto>();
            CreateMap<IngredientDto, Ingredient>();
            CreateMap<DetailIngredientDish, DetailIngredientDishDto>();
            CreateMap<DetailIngredientDishDto, DetailIngredientDish>();
            CreateMap<TypeDish, TypeDishDto>();
            CreateMap<TypeDishDto, TypeDish>();
            CreateMap<DetailTypeDish, DetailTypeDishDto>();
            CreateMap<DetailTypeDishDto, DetailTypeDish>();
            CreateMap<Recipe, RecipeDto>();
            CreateMap<RecipeDto, Recipe>();
        }
    }
}
