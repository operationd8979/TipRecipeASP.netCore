using Microsoft.AspNetCore.Http.HttpResults;
using TipRecipe.Entities;
using TipRecipe.Interfaces;
using TipRecipe.Models.Dto;
using TipRecipe.Repositorys;

namespace TipRecipe.Services
{
    public class DishService
    {

        private readonly IDishRepository _dishRepository;
        private readonly IIngredientRepository _ingredientRepository;
        private readonly ITypeDishRepository _typeDishRepository;

        public DishService(
            IDishRepository dishRepository,
            IIngredientRepository ingredientRepository,
            ITypeDishRepository typeDishRepository) {
            this._dishRepository = dishRepository ?? throw new ArgumentNullException(nameof(dishRepository));
            this._ingredientRepository = ingredientRepository ?? throw new ArgumentNullException(nameof(ingredientRepository));
            this._typeDishRepository = typeDishRepository ?? throw new ArgumentNullException(nameof(typeDishRepository));
        }

        public async Task<IEnumerable<Dish>> GetAllAsync()
        {
            return await this._dishRepository.GetAllAsync();
        }

        public async Task<IEnumerable<Dish>> GetDishWithFilterAsync(
            string query,
            string ingredients,
            string types,
            int offset,
            int limit,
            string orderBy)
        {
            int[] ingredientsArray = [];
            int[] typesArray = [];
            if (!string.IsNullOrEmpty(ingredients))
            {
                ingredientsArray = ingredients!.Split(",").Select(i => int.Parse(i)).ToArray();
            }
            if (!string.IsNullOrEmpty(types))
            {
                typesArray = types!.Split(",").Select(t => int.Parse(t)).ToArray();
            }
            return await this._dishRepository.GetWithFilterAsync(query,ingredientsArray,typesArray,offset,limit,orderBy);
        }

        public async Task<Dish?> GetByIdAsync(string dishID)
        {
            return await this._dishRepository.GetByIDAsync(dishID);
        }

        public async Task<bool> AddDishAsync(Dish dish)
        {
            ICollection<DetailIngredientDish> detailIngredientDishes = dish.DetailIngredientDishes;
            ICollection<DetailTypeDish> detailTypeDishes = dish.DetailTypeDishes;
            dish.DetailIngredientDishes = [];
            dish.DetailTypeDishes = [];
            this._dishRepository.Add(dish);
            if (this._dishRepository.SaveChanges())
            {
                dish.DetailIngredientDishes = detailIngredientDishes;
                dish.DetailTypeDishes = detailTypeDishes;
            }
            return await this._dishRepository.SaveChangesAsync();
        }

        public async Task<bool> UpdateDishAsync(string dishID, Dish updatedDish)
        {
            Dish? findDish = await this._dishRepository.GetByIDAsync(dishID);
            if(findDish != null)
            {
                findDish.DishName = updatedDish.DishName;
                findDish.Summary = updatedDish.Summary;
                findDish.UrlPhoto = updatedDish.UrlPhoto;
                findDish.AvgRating = updatedDish.AvgRating;
                findDish.DetailIngredientDishes = updatedDish.DetailIngredientDishes;
                findDish.DetailTypeDishes = updatedDish.DetailTypeDishes;
                return await this._dishRepository.SaveChangesAsync();
            }
            return false;
        }

        public async Task<IEnumerable<Ingredient>> GetIngredientsAsync()
        {
            return await this._ingredientRepository.GetAllAsync();
        }

        public async Task<IEnumerable<TypeDish>> GetTypesAsync()
        {
            return await this._typeDishRepository.GetAllAsync();
        }

    }
}
