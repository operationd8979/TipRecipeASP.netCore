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

        public async Task<IEnumerable<Ingredient>> GetAllAsync()
        {
            return await this._context.Ingredients.ToListAsync();
        }

        public int SaveChanges()
        {
            return this._context.SaveChanges();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await this._context.SaveChangesAsync();
        }
    }
}
