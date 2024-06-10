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
            CreateMap<CreateDishDto, Dish>()
                .ForMember(dst => dst.DetailIngredientDishes, 
                    opt => opt.MapFrom(src => src.DetailIngredientDishes!.Select(did => new DetailIngredientDish(did.Ingredient!.IngredientId, did.Amount, did.Unit!))))
                .ForMember(dst => dst.DetailTypeDishes,
                    opt => opt.MapFrom(src => src.DetailTypeDishes!.Select(dtd => new DetailTypeDish(dtd.Type!.TypeID))));
            
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
