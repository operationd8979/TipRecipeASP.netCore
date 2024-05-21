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
            return new DishDto(dish.DishID,dish.DishName,dish.Summary,dish.UrlPhoto,dish.AvgRating);
        }

        public Dish Translate(DishDto dishDto)
        {
            return new Dish(dishDto.DishID,dishDto.DishName,dishDto.Summary,dishDto.UrlPhoto);
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
