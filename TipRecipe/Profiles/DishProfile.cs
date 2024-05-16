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
        }
    }
}
