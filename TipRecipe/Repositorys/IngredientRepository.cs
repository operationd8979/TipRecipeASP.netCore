using Microsoft.EntityFrameworkCore;
using TipRecipe.DbContexts;
using TipRecipe.Entities;
using TipRecipe.Interfaces;

namespace TipRecipe.Repositorys
{
    public class IngredientRepository : IIngredientRepository
    {
        private readonly ApplicationDbContext _context;
        public IngredientRepository(ApplicationDbContext applicationDbContext)
        {
            _context = applicationDbContext ?? throw new ArgumentNullException(nameof(applicationDbContext));
        }

        public void Add(Ingredient newObject)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Ingredient>> GetAllAsync()
        {
            return await this._context.Ingredients.ToListAsync();
        }

        public IAsyncEnumerable<Ingredient> GetAllEnumerableAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Ingredient?> GetByIDAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetCountAsync(string query)
        {
            throw new NotImplementedException();
        }

        public int SaveChanges()
        {
            return this._context.SaveChanges();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await this._context.SaveChangesAsync();
        }

        public void Update(Ingredient updateObject)
        {
            throw new NotImplementedException();
        }

        bool IDataRepository<Ingredient>.SaveChanges()
        {
            throw new NotImplementedException();
        }

        Task<bool> IDataRepository<Ingredient>.SaveChangesAsync()
        {
            throw new NotImplementedException();
        }
    }
}
