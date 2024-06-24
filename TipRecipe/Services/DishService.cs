using Serilog;
using System.Collections.Concurrent;
using System.Data;
using TipRecipe.Entities;
using TipRecipe.Extensions;
using TipRecipe.Interfaces;
using TipRecipe.Models;
using TipRecipe.Models.HttpExceptions;


namespace TipRecipe.Services
{
    public class DishService
    {

        private readonly IDishRepository _dishRepository;
        private readonly IIngredientRepository _ingredientRepository;
        private readonly ITypeDishRepository _typeDishRepository;

        private readonly CachedRatingScoreService _cachedRatingService;

        public DishService(
            IDishRepository dishRepository,
            IIngredientRepository ingredientRepository,
            ITypeDishRepository typeDishRepository,
            CachedRatingScoreService cachedRatingScoreService) {
            this._dishRepository = dishRepository ?? throw new ArgumentNullException(nameof(dishRepository));
            this._ingredientRepository = ingredientRepository ?? throw new ArgumentNullException(nameof(ingredientRepository));
            this._typeDishRepository = typeDishRepository ?? throw new ArgumentNullException(nameof(typeDishRepository));
            this._cachedRatingService = cachedRatingScoreService ?? throw new ArgumentNullException(nameof(cachedRatingScoreService));
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

        public async Task<IEnumerable<Dish>> GetDishByAdminAsync(
            string query,
            int offset,
            int limit,
            string orderBy)
        {
            IEnumerable<Dish> dishes = await this._dishRepository.GetDishByAdminAsync(query, offset, limit, orderBy);
            return dishes;
        }

        public async Task<IEnumerable<Dish>> GetSomeByIDsAsync(IEnumerable<string> dishIDs)
        {
            return await this._dishRepository.GetSomeByIDsAsync(dishIDs);
        }

        public async Task<IEnumerable<Dish>> GetRecommendDishesAsync(string userID)
        {
            Dictionary<string, Dictionary<string, CachedRating>>? ratingMap = await _cachedRatingService.GetRatingsAsync();
            if (ratingMap == null || !ratingMap.ContainsKey(userID))
            {
                ratingMap = [];
                IEnumerable<UserDishRating> ratings = await GetUserDishRatingsAsync();
                foreach (var rating in ratings)
                {
                    if (ratingMap.TryGetValue(rating.UserID!, out var value))
                    {
                        value.Add(rating.DishID!, new CachedRating(rating.RatingScore / 10, rating.RatedAt is not null));
                    }
                    else
                    {
                        Dictionary<string, CachedRating> newValue = new();
                        newValue.Add(rating.DishID!, new CachedRating(rating.RatingScore / 10, rating.RatedAt is not null));
                        ratingMap.Add(rating.UserID!, newValue);
                    }
                }
            }
            Dictionary<string, CachedRating> currentUser = ratingMap.GetValueOrDefault(userID)!;
            List<KeyValuePair<string,CachedRating>> list = currentUser.ToList();
            list.Sort((pairA, pairB) =>
            {
                return pairB.Value.RatingScore.CompareTo(pairA.Value.RatingScore);
            });
            return await this._dishRepository.GetSomeByIDsAsync(list.Select(i=>i.Key).Take(10));
        }

        public async Task<int> GetCountDishesAsync(string query)
        {
            return await this._dishRepository.GetCountAsync(query);
        }

        private async Task<IEnumerable<Dish>> ImplementRatingScoreDishes(IEnumerable<Dish> dishes, string userID)
        {
            Dictionary<string, Dictionary<string,CachedRating>>? ratingMap = await _cachedRatingService.GetRatingsAsync();
            bool shouldResetRatingMap = false;
            bool shouldVectorizeRatingMap = false;
            if(ratingMap != null)
            {
                if (ratingMap.TryGetValue(userID, out var value))
                {
                    foreach (var dish in dishes)
                    {
                        if (value.TryGetValue(dish.DishID, out var rating))
                        {
                            if (!rating.IsRated && !rating.IsPreRated)
                            {
                                shouldVectorizeRatingMap = true;
                            }
                        }
                        else
                        {
                            shouldResetRatingMap = true;
                            shouldVectorizeRatingMap = true;
                            break;
                        }
                    }
                }
                else
                {
                    shouldResetRatingMap = true;
                    shouldVectorizeRatingMap = true;
                }
            }
            if (ratingMap == null || shouldResetRatingMap)
            {
                shouldVectorizeRatingMap = true;
                ratingMap = [];
                IEnumerable<UserDishRating> ratings = await GetUserDishRatingsAsync();
                foreach (var rating in ratings)
                {
                    if(ratingMap.TryGetValue(rating.UserID!, out var value))
                    {
                        value.Add(rating.DishID!, new CachedRating(rating.RatingScore / 10, rating.RatedAt is not null));
                    }
                    else
                    {
                        Dictionary<string, CachedRating> newValue = new();
                        newValue.Add(rating.DishID!, new CachedRating(rating.RatingScore / 10, rating.RatedAt is not null));
                        ratingMap.Add(rating.UserID!, newValue);
                    }
                }
                await _cachedRatingService.OverwriteRatingAsync(ratingMap);
            }

            List<(string, double)> vectorList = new();
            if (shouldVectorizeRatingMap)
            {
                vectorList = VectorizeRatingMap(ratingMap, userID);
            }

            IEnumerable<float> rawDishs = await _dishRepository.GetAvgScoreByIDs(dishes.Select(d => d.DishID).ToList());
            List<object> preRatingCal = new List<object>();
            bool isUpdateRating = false;
            for(int i = 0; i < dishes.Count(); i++)
            {
                Dish dish = dishes.ElementAt(i);
                dish.RatingScore = ratingMap.GetValueOrDefault(userID)!.GetValueOrDefault(dish.DishID)!.RatingScore;
                dish.isRated = ratingMap.GetValueOrDefault(userID)!.GetValueOrDefault(dish.DishID)!.IsRated;
                if (!dish.isRated && !ratingMap.GetValueOrDefault(userID)!.GetValueOrDefault(dish.DishID)!.IsPreRated)
                {
                    isUpdateRating = true;
                    ratingMap.GetValueOrDefault(userID)![dish.DishID].IsPreRated = true;
                    foreach (var vector in vectorList)
                    {
                        dish.RatingScore = ratingMap.GetValueOrDefault(vector.Item1)!.GetValueOrDefault(dish.DishID)!.RatingScore;
                        if (dish.RatingScore.IsApproximately(0f))
                        {
                            ratingMap.GetValueOrDefault(userID)![dish.DishID].RatingScore = dish.RatingScore;
                            preRatingCal.Add(new { dish.DishID, dish.RatingScore });
                            break;
                        }
                    }
                }
                dish.RatingScore = dish.RatingScore*10 + rawDishs.ElementAt(i);
            }
            if (isUpdateRating)
            {
                await _cachedRatingService.UpdateRatingsAsync(ratingMap);
            }
            return dishes;
        }

        private List<(string, double)> VectorizeRatingMap(
            Dictionary<string,Dictionary<string,CachedRating>> ratingMap,
            string userID
            )
        {
            List<(string, double)> vectorList;
            Log.Information("Vectorize rating map!");
            Dictionary<string, CachedRating> dataCurrentUser = ratingMap.GetValueOrDefault(userID)!;
            var arrayDishIDs = dataCurrentUser.Keys.ToArray();

            ConcurrentBag<(string, double)> vectorConcurrent = new ConcurrentBag<(string, double)>();
            (string, double) vectorCurrentUser = new(string.Empty, 0);
            object lockObj = new object();

            Parallel.ForEach(ratingMap, dataUser =>
            {
                var (dotResult, magnitudeA, magnitudeB) = CalculateSimilarityMetrics(dataUser.Value, dataCurrentUser, arrayDishIDs);
                double result = dotResult / (Math.Sqrt(magnitudeA) * Math.Sqrt(magnitudeB));
                if (double.IsNaN(result))
                {
                    result = 1;
                }
                if (dataUser.Key.Equals(userID))
                {
                    lock (lockObj)
                    {
                        vectorCurrentUser = (userID, result);
                    }
                }
                else
                {
                    vectorConcurrent.Add((dataUser.Key, result));
                }
            });

            vectorList = vectorConcurrent.ToList();
            vectorList.Sort((vectorA, vectorB) =>
            {
                double a = Math.Abs(vectorA.Item2) - Math.Abs(vectorCurrentUser.Item2);
                double b = Math.Abs(vectorB.Item2) - Math.Abs(vectorCurrentUser.Item2);
                return a.CompareTo(b);
            });

            return vectorList;
        }

        private (double DotResult, double MagnitudeA, double MagnitudeB) CalculateSimilarityMetrics(
            Dictionary<string, CachedRating> ratingsA,
            Dictionary<string, CachedRating> ratingsB,
            string[] dishIDs)
        {

            double dotResult = 0, magnitudeA = 0, magnitudeB = 0;

            foreach (var dishID in dishIDs)
            {
                var ratingA = ratingsA.GetValueOrDefault(dishID)?.RatingScore ?? 0;
                var ratingB = ratingsB.GetValueOrDefault(dishID)?.RatingScore ?? 0;

                dotResult += ratingA * ratingB;
                magnitudeA += Math.Pow(ratingA, 2);
                magnitudeB += Math.Pow(ratingB, 2);
            }

            return (dotResult, magnitudeA, magnitudeB);
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
                findDish.UrlPhoto = String.IsNullOrEmpty(updatedDish.UrlPhoto) ? findDish.UrlPhoto : updatedDish.UrlPhoto;
                findDish.DetailIngredientDishes = updatedDish.DetailIngredientDishes;
                findDish.DetailTypeDishes = updatedDish.DetailTypeDishes;
                findDish.Recipe = updatedDish.Recipe;
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
                findRating = new Rating(userID,dishID,ratingScore,DateTime.Now);
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
            await _cachedRatingService.UpdateRatingAsync(new CachedRating((ratingScore-findDish.AvgRating)/10,true), userID, dishID);
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
                findDish.IsDeleted = !findDish.IsDeleted;
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
