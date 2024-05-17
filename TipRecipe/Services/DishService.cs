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

        public async Task<Dish> GetByIdAsync(string dishID)
        {
            return await this._dishRepository.GetByIDAsync(dishID);
        }

        public async Task<int> AddDishAsync(Dish dish)
        {
            IList<DetailIngredientDish> detailIngredientDishes = dish.DetailIngredientDishes.ToList();
            IList<DetailTypeDish> detailTypeDishes = dish.DetailTypeDishes.ToList();
            dish.DetailIngredientDishes = [];
            dish.DetailTypeDishes = [];
            await this._dishRepository.AddAsync(dish);
            if (this._dishRepository.SaveChanges() >= 0)
            {
                foreach (DetailIngredientDish item in detailIngredientDishes)
                {
                    item.IngredientID = item.Ingredient!.IngredientID;
                    //ensure not overide IngredientName from request body
                    item.Ingredient = null;
                    dish.DetailIngredientDishes.Add(item);
                }
                foreach (DetailTypeDish item in detailTypeDishes)
                {
                    item.TypeID = item.Type!.TypeID;
                    //ensure not overide TypeName from request body
                    item.Type = null;
                    dish.DetailTypeDishes.Add(item);
                }
            }
            return this._dishRepository.SaveChanges();
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
