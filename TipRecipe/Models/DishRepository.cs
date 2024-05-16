using TipRecipe.DbContexts;
using TipRecipe.Entities;
using TipRecipe.Interfaces;

namespace TipRecipe.Models
{
    public class DishRepository : IPageinationDataRepository<Dish, String>
    {
        private readonly ApplicationDbContext _context;
        public DishRepository(ApplicationDbContext applicationDbContext)
        {
            this._context = applicationDbContext ?? throw new ArgumentNullException(nameof(applicationDbContext));
        }


        public Dish Add(Dish entity)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Dish> GetAll(int offset, int limit)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Dish> GetAll()
        {
            return this._context.Dishes.ToList();
        }

        public Dish GetByID(String id)
        {
            throw new NotImplementedException();
        }

        public Dish Update(Dish entity)
        {
            throw new NotImplementedException();
        }
    }
}
