using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TipRecipe.DbContexts;
using TipRecipe.Entities;
using TipRecipe.Interfaces;
using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore.Metadata.Internal;


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
                .Include(d => d.DetailIngredientDishes).ThenInclude(did => did.Ingredient)
                .ToListAsync();
        }

        public async Task<IEnumerable<Dish>> GetWithFilterAsync(string query, int[] ingredients, int[] types, int offset, int limit, string orderBy)
        {
            var dishesQuery = _context.Dishes.AsQueryable();
            if (!string.IsNullOrEmpty(query))
            {
                dishesQuery = dishesQuery.Where(d => d.DishName.Contains(query));
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
                .Include(d => d.DetailIngredientDishes).ThenInclude(did => did.Ingredient)
                .Include(d => d.DetailTypeDishes).ThenInclude(dtd => dtd.Type)
                .Skip(offset).Take(limit).ToListAsync();
        }

        public async Task<Dish?> GetByIDAsync(string dishID)
        {
            return await this._context.Dishes
                .Include(d=>d.Recipe)
                .Include(d=> d.DetailIngredientDishes).ThenInclude(did=>did.Ingredient)
                .Include(d => d.DetailTypeDishes).ThenInclude(dtd => dtd.Type)
                .FirstOrDefaultAsync(d => d.DishID == dishID);
        }


        public bool SaveChanges()
        {
            return this._context.SaveChanges() >= 0;
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await this._context.SaveChangesAsync() >= 0;
        }

       
    }
}
