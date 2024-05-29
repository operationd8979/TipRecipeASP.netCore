using Microsoft.Data.SqlClient;
using Serilog;
using System.Collections;
using System.Data;
using TipRecipe.Entities;
using TipRecipe.Interfaces;
using TipRecipe.Models;
using TipRecipe.Models.Dto;
using TipRecipe.Models.HttpExceptions;


namespace TipRecipe.Services
{
    public class DishService
    {

        private readonly IDishRepository _dishRepository;
        private readonly IIngredientRepository _ingredientRepository;
        private readonly ITypeDishRepository _typeDishRepository;

        private readonly CachingFileService _cachingFileService;

        public DishService(
            IDishRepository dishRepository,
            IIngredientRepository ingredientRepository,
            ITypeDishRepository typeDishRepository,
            CachingFileService cachingFileService) {
            this._dishRepository = dishRepository ?? throw new ArgumentNullException(nameof(dishRepository));
            this._ingredientRepository = ingredientRepository ?? throw new ArgumentNullException(nameof(ingredientRepository));
            this._typeDishRepository = typeDishRepository ?? throw new ArgumentNullException(nameof(typeDishRepository));
            this._cachingFileService = cachingFileService ?? throw new ArgumentNullException(nameof(cachingFileService));
        }

        public async Task<IEnumerable<Dish>> GetAllAsync()
        {
            return await this._dishRepository.GetAllAsync();
        }

        public IAsyncEnumerable<Dish> GetAllEnumerableAsync()
        {
            return this._dishRepository.GetAllEnumerableAsync();
        }

        public async Task<IEnumerable<Dish>> GetDishWithFilterAsync(
            string query,
            string ingredients,
            string types,
            int offset,
            int limit,
            string orderBy,
            string userID)
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
            IEnumerable<Dish> dishes = await this._dishRepository.GetWithFilterAsync(query,ingredientsArray,typesArray,offset,limit,orderBy);
            return await ImplementRatingScoreDishes(dishes,userID);
        }

        private async Task<IEnumerable<Dish>> ImplementRatingScoreDishes(IEnumerable<Dish> dishes, string userID)
        {
            Dictionary<string, Dictionary<string,float>>? ratingMap = await _cachingFileService.GetAsync<Dictionary<string, Dictionary<string, float>>>("RATINGS");
            bool shouldResetRatingMap = false;
            if(ratingMap != null)
            {
                if (ratingMap.TryGetValue(userID, out var value))
                {
                    foreach (var dish in dishes)
                    {
                        if (!value.ContainsKey(dish.DishID))
                        {
                            shouldResetRatingMap = true;
                            break;
                        }
                    }
                }
                else
                {
                    shouldResetRatingMap = true;
                }
            }
            if (ratingMap == null || shouldResetRatingMap)
            {
                ratingMap = [];
                IEnumerable<UserDishRating> ratings = await GetUserDishRatingsAsync();
                foreach (var rating in ratings)
                {
                    if(ratingMap.TryGetValue(rating.UserID!, out var value))
                    {
                        value.Add(rating.DishID!, rating.RatingScore / 10);
                    }
                    else
                    {
                        Dictionary<string, float> newValue = new();
                        newValue.Add(rating.DishID!, rating.RatingScore / 10);
                        ratingMap.Add(rating.UserID!, newValue);
                    }
                }
                await _cachingFileService.SetAsync("RATINGS", ratingMap, TimeSpan.FromMinutes(15));
            }
            List<(string, double)> vectorList = new();
            Dictionary<string, float> dataCurrentUser = ratingMap.GetValueOrDefault(userID)!;
            var arrayDishIDs = dataCurrentUser.Keys.ToArray();
            (string, double) vectorCurrentUser = new();
            foreach(var dataUser in ratingMap)
            {
                double dotResult = 0;
                double magnitudeA = 0;
                double magnitudeB = 0;
                foreach (var item in arrayDishIDs)
                {
                    dotResult += dataUser.Value.GetValueOrDefault(item) * dataCurrentUser.GetValueOrDefault(item);
                    magnitudeA += Math.Pow(dataUser.Value.GetValueOrDefault(item), 2);
                    magnitudeB += Math.Pow(dataCurrentUser.GetValueOrDefault(item), 2);
                }
                double result = dotResult / (Math.Sqrt(magnitudeA) * Math.Sqrt(magnitudeB));
                if (double.IsNaN(result))
                {
                    result = 1;
                }
                if (dataUser.Key.Equals(userID))
                {
                    vectorCurrentUser = (userID, result);
                }
                else
                {
                    vectorList.Add((dataUser.Key, result));
                }
            }
            //Parallel.ForEach(ratingMap, dataUser =>
            //{
            //    double dotResult = 0;
            //    double magnitudeA = 0;
            //    double magnitudeB = 0;
            //    foreach (var item in arrayDishIDs)
            //    {
            //        dotResult += dataUser.Value.GetValueOrDefault(item) * dataCurrentUser.GetValueOrDefault(item);
            //        magnitudeA += Math.Pow(dataUser.Value.GetValueOrDefault(item), 2);
            //        magnitudeB += Math.Pow(dataCurrentUser.GetValueOrDefault(item), 2);
            //    }
            //    double result = dotResult / (Math.Sqrt(magnitudeA) * Math.Sqrt(magnitudeB));
            //    if (double.IsNaN(result))
            //    {
            //        result = 1;
            //    }
            //    if (dataUser.Key.Equals(userID))
            //    {
            //        vectorCurrentUser = (userID, result);
            //    }
            //    else
            //    {
            //        vectorList.Add((dataUser.Key, result));
            //    }
            //});
            vectorList.Sort((vectorA, vectorB) =>
            {
                double a = Math.Abs(vectorA.Item2) - Math.Abs(vectorCurrentUser.Item2);
                double b = Math.Abs(vectorB.Item2) - Math.Abs(vectorCurrentUser.Item2);
                return a.CompareTo(b);
            });
            IEnumerable<Dish> rawDishs = await _dishRepository.GetDishsByListID(dishes.Select(d => d.DishID).ToList());
            List<object> preRatingCal = new List<object>();
            for(int i = 0; i < dishes.Count(); i++)
            {
                Dish dish = dishes.ElementAt(i);
                dish.RatingScore = ratingMap.GetValueOrDefault(userID)!.GetValueOrDefault(dish.DishID);
                if (dish.RatingScore == 0)
                {
                    foreach (var vector in vectorList)
                    {
                        dish.RatingScore = ratingMap.GetValueOrDefault(vector.Item1)!.GetValueOrDefault(dish.DishID);
                        if (dish.RatingScore != 0)
                        {
                            ratingMap.GetValueOrDefault(userID)![dish.DishID] = dish.RatingScore??0;
                            preRatingCal.Add(new { dish.DishID, dish.RatingScore });
                            break;
                        }
                    }
                }
                dish.RatingScore = dish.RatingScore*10 + rawDishs.ElementAt(i).AvgRating;
            }
            if (preRatingCal.Count() > 0)
            {
                await _cachingFileService.SetAsync("RATINGS", ratingMap, TimeSpan.FromMinutes(15));
            }
            return dishes;
        }

        public async Task<Dish?> GetByIdAsync(string dishID)
        {
            Dish? dish = await this._dishRepository.GetByIDAsync(dishID);
            return dish;
        }

        public async Task<Dish?> GetDishWithRatingByIDAsync(string dishID, string userID)
        {
            Dish? dish = await this._dishRepository.GetByIDAsync(dishID);
            float? rating = await this._dishRepository.GetRatingOfDish(dishID, userID);
            if(rating is not null)
            {
                dish!.RatingScore = rating.Value;
                dish!.isRated = true;
            }
            return dish;
        }


        public async Task<bool> AddDishAsync(Dish dish)
        {
            this._dishRepository.Add(dish);
            return await this._dishRepository.SaveChangesAsync();
        }

        public async Task<bool> UpdateDishAsync(string dishID, Dish updatedDish)
        {
            Dish? findDish = await this._dishRepository.GetByIDAsync(dishID);
            if(findDish is not null)
            {
                findDish.DishName = updatedDish.DishName;
                findDish.Summary = updatedDish.Summary;
                findDish.UrlPhoto = updatedDish.UrlPhoto;
                findDish.DetailIngredientDishes = updatedDish.DetailIngredientDishes;
                findDish.DetailTypeDishes = updatedDish.DetailTypeDishes;
                return await this._dishRepository.SaveChangesAsync();
            }
            return false;
        }

        public async Task<bool> RatingDishAsync(string dishID, float ratingScore, string userID)
        {
            Dish? findDish = await this._dishRepository.GetByIDAsync(dishID);
            if(findDish is null)
            {
                throw new NotFoundException("Dish not found");
            }
            Rating? findRating = await this._dishRepository.GetRatingDishAsync(dishID, userID);
            if (findRating is null)
            {
                findRating = new Rating(userID,dishID,ratingScore,0,DateTime.Now);
                findRating.Dish = findDish;
                this._dishRepository.AddRating(findRating);
            }
            else
            {
                findRating.DishID = dishID;
                findRating.UserID = userID;
                findRating.RatingScore = ratingScore;
                findRating.RatedAt = DateTime.Now;
            }
            return await this._dishRepository.SaveChangesAsync();
        }

        public async Task<IEnumerable<Ingredient>> GetIngredientsAsync()
        {
            return await this._ingredientRepository.GetAllAsync();
        }

        public async Task<IEnumerable<TypeDish>> GetTypesAsync()
        {
            return await this._typeDishRepository.GetAllAsync();
        }

        public bool SaveChanges()
        {
            return this._dishRepository.SaveChanges();
        }

        public async Task<bool> SaveChangeAsync()
        {
            return await this._dishRepository.SaveChangesAsync();
        }

        private async Task<IEnumerable<UserDishRating>> GetUserDishRatingsAsync()
        {
            return await _dishRepository.GetUserDishRatingsAsync();
        }

        public async Task<bool> DeleteDishAsync(string dishID)
        {
            Dish? findDish = await this._dishRepository.GetByIDAsync(dishID);
            if(findDish is not null)
            {
                findDish.IsDeleted = true;
                return await this._dishRepository.SaveChangesAsync();
            }
            return false;
        }

        public async Task UpdateAverageScoreDishes()
        {
            await this._dishRepository.UpdateAverageScoreDishes();
        }
    }
}
