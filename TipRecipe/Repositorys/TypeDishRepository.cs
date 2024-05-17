using Microsoft.EntityFrameworkCore;
using TipRecipe.DbContexts;
using TipRecipe.Entities;
using TipRecipe.Interfaces;

namespace TipRecipe.Repositorys
{
    public class TypeDishRepository : ITypeDishRepository
    {

        private readonly ApplicationDbContext _context;

        public TypeDishRepository(ApplicationDbContext applicationDbContext)
        {
            _context = applicationDbContext ?? throw new ArgumentNullException(nameof(applicationDbContext));
        }


        public async Task<IEnumerable<TypeDish>> GetAllAsync()
        {
            return await this._context.TypeDishs.ToArrayAsync();
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
