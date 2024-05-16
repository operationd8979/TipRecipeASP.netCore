using Microsoft.EntityFrameworkCore;
using TipRecipe.Entities;

namespace TipRecipe.DbContexts
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Dish> Dishes { get; set; }
    }
}
