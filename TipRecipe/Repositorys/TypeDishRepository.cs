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

        public void Add(TypeDish newObject)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<TypeDish>> GetAllAsync()
        {
            return await this._context.TypeDishs.ToArrayAsync();
        }

        public IAsyncEnumerable<TypeDish> GetAllEnumerableAsync()
        {
            throw new NotImplementedException();
        }

        public Task<TypeDish?> GetByIDAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetCountAsync()
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

        public void Update(TypeDish updateObject)
        {
            throw new NotImplementedException();
        }

        bool IDataRepository<TypeDish>.SaveChanges()
        {
            throw new NotImplementedException();
        }

        Task<bool> IDataRepository<TypeDish>.SaveChangesAsync()
        {
            throw new NotImplementedException();
        }
    }
}
