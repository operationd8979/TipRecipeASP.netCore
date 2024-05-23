using TipRecipe.Entities;
using TipRecipe.Interfaces;
using TipRecipe.Models.Dto;

namespace TipRecipe.Helper
{
    public class DishTranslateMapper : ITranslateMapper<Dish, DishDto>
    {
        public bool Compare(Dish a, DishDto b)
        {
            return a.Equals(this.Translate(b));
        }

        public DishDto Translate(Dish dish)
        {
            DishDto dishDto = new DishDto();
            dishDto.DishID = dish.DishID;
            dishDto.DishName = dish.DishName;
            dishDto.Summary = dish.Summary;
            dishDto.UrlPhoto = dish.UrlPhoto;
            dishDto.AvgRating = dish.AvgRating;
            return dishDto;
        }

        public Dish Translate(DishDto dishDto)
        {
            Dish dish = new Dish();
            dish.DishID = dishDto.DishID!;
            dish.DishName = dishDto.DishName;
            dish.Summary = dishDto.Summary;
            dish.UrlPhoto = dishDto.UrlPhoto;
            dish.AvgRating = dishDto.AvgRating;
            return dish;
        }

        public IList<DishDto> TranslateList(IList<Dish> dishList)
        {
            IList<DishDto> dishDtoList = new List<DishDto>();
            foreach (Dish dish in dishList)
            {
                dishDtoList.Add(Translate(dish));
            }
            return dishDtoList;
        }

        public IList<Dish> TranslateList(IList<DishDto> dishDtoLish)
        {
            IList<Dish> dishList = new List<Dish>();
            foreach (DishDto dishDto in dishDtoLish)
            {
                dishList.Add(Translate(dishDto));
            }
            return dishList;
        }



    }
}
