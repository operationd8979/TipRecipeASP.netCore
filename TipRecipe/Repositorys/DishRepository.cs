using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TipRecipe.DbContexts;
using TipRecipe.Entities;
using TipRecipe.Interfaces;
using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Data.SqlClient;
using System.Data;
using TipRecipe.Models;
using Dapper;


namespace TipRecipe.Repositorys
{
    public class DishRepository : IDishRepository
    {
        private readonly ApplicationDbContext _context;

        public DishRepository(ApplicationDbContext applicationDbContext)
        {
            _context = applicationDbContext ?? throw new ArgumentNullException(nameof(applicationDbContext));
        }

        public void Add(Dish newDish)
        {
            this._context.Dishes.Add(newDish);
        }

        public void Update(Dish updateDish)
        {
            this._context.Dishes.Update(updateDish);
        }

        public async Task<IEnumerable<Dish>> GetAllAsync()
        {
            return await this._context.Dishes
                .Where(d => d.IsDeleted == false)
                .Include(d => d.DetailIngredientDishes).ThenInclude(did => did.Ingredient)
                .Include(d => d.DetailTypeDishes).ThenInclude(dtd => dtd.Type)
                .ToListAsync();
        }

        public IAsyncEnumerable<Dish> GetAllEnumerableAsync()
        {
            return _context.Dishes
                .Where(d => d.IsDeleted == false)
                .Include(d => d.DetailIngredientDishes).ThenInclude(did => did.Ingredient)
                .Include(d => d.DetailTypeDishes).ThenInclude(dtd => dtd.Type)
                .AsAsyncEnumerable();
        }

        public async Task<IEnumerable<Dish>> GetWithFilterAsync(string query, int[] ingredients, int[] types, int offset, int limit, string orderBy)
        {
            var dishesQuery = _context.Dishes.AsQueryable();
            if (!string.IsNullOrEmpty(query))
            {
                dishesQuery = dishesQuery.Where(d => d.DishName!.Contains(query));
            }
            if (ingredients != null && ingredients.Any())
            {
                dishesQuery = dishesQuery.Where(d => d.DetailIngredientDishes.Any(did => ingredients.Contains(did.IngredientID)));
            }
            if (types != null && types.Any())
            {
                dishesQuery = dishesQuery.Where(d => d.DetailTypeDishes.Any(dtd => types.Contains(dtd.TypeID)));
            }

            if (!string.IsNullOrEmpty(orderBy))
            {
                bool descending = orderBy.StartsWith("-");
                string propertyName = descending ? orderBy.Substring(1) : orderBy;

                if (descending)
                {
                    dishesQuery = dishesQuery.OrderBy($"{propertyName} descending");
                }
                else
                {
                    dishesQuery = dishesQuery.OrderBy(propertyName);
                }
            }
            return await dishesQuery
                .Where(d => d.IsDeleted == false)
                .Include(d => d.DetailIngredientDishes).ThenInclude(did => did.Ingredient)
                .Include(d => d.DetailTypeDishes).ThenInclude(dtd => dtd.Type)
                .Skip(offset).Take(limit).ToListAsync();
        }

        public async Task<Dish?> GetByIDAsync(string dishID)
        {
            return await this._context.Dishes
                .Include(d => d.Recipe)
                .Include(d => d.DetailIngredientDishes).ThenInclude(did => did.Ingredient)
                .Include(d => d.DetailTypeDishes).ThenInclude(dtd => dtd.Type)
                .FirstOrDefaultAsync(d => d.DishID == dishID && d.IsDeleted == false);
        }

        public async Task<Rating?> GetRatingDishAsync(string dishID, string userID)
        {
            return await this._context.Ratings.FirstOrDefaultAsync(r => r.DishID == dishID && r.UserID == userID);
        }

        public void AddRating(Rating rating)
        {
            this._context.Ratings.Add(rating);
        }


        public bool SaveChanges()
        {
            return this._context.SaveChanges() >= 0;
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await this._context.SaveChangesAsync() >= 0;
        }


        public async Task<IEnumerable<Dish>> GetDishsByListID(List<string> dishIDs)
        {
            return await _context.Dishes.Where(d => dishIDs.Contains(d.DishID)).ToListAsync();
        }

        public async Task<IEnumerable<UserDishRating>> GetUserDishRatingsAsync()
        {
            string sqlQuery = @"
                SELECT u.UserID, d.DishID, COALESCE(r.RatingScore, d.AvgRating) - d.AvgRating AS RatingScore, r.RatedAt
                FROM Users u
                CROSS JOIN Dishes d
                LEFT JOIN Ratings r ON r.UserID = u.UserID AND r.DishID = d.DishID
                ORDER BY d.DishID, u.UserID";
            return await _context.Database.SqlQueryRaw<UserDishRating>(sqlQuery).ToListAsync();
        }

        public async Task UpdateAverageScoreDishes()
        {
            string sqlQuery = @"
                UPDATE d
                SET d.AvgRating = avg_ratings.avgRating
                FROM dbo.Dishes d
                INNER JOIN (
                    SELECT DishID, AVG(RatingScore) AS avgRating
                    FROM dbo.Ratings
                    GROUP BY DishID
                ) AS avg_ratings ON d.DishID = avg_ratings.DishID;";
            await _context.Database.ExecuteSqlRawAsync(sqlQuery);
        }

        public async Task<float?> GetRatingOfDish(string dishID, string userID)
        {
            string sqlQuery = @"
                SELECT COALESCE(r.RatingScore, 0) AS RatingScore
                FROM dbo.Dishes d
                LEFT JOIN dbo.Ratings r ON r.DishID = d.DishID AND r.UserID = @UserID
                WHERE d.DishID = @DishID";
            var rating = await _context.Ratings
                                       .FromSqlRaw(sqlQuery, new SqlParameter("@DishID", dishID), new SqlParameter("@UserID", userID))
                                       .Select(r => r.RatingScore)
                                       .FirstOrDefaultAsync();
            return rating;
        }


    }
}
