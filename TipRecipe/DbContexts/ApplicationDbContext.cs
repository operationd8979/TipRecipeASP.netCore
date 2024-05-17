using Microsoft.EntityFrameworkCore;
using TipRecipe.Entities;
using TipRecipe.Helper;

namespace TipRecipe.DbContexts
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Dish> Dishes { get; set; }
        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<DetailIngredientDish> DetailIngredientDishes { get; set; }
        public DbSet<TypeDish> TypeDishs { get; set; }
        public DbSet<DetailTypeDish> DetailTypeDishes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DetailIngredientDish>()
                .HasKey(did => new { did.DishID, did.IngredientID });
            modelBuilder.Entity<DetailTypeDish>()
                .HasKey(dtd => new { dtd.DishID, dtd.TypeID });

        }

        public override int SaveChanges()
        {
            GenerateIds();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            GenerateIds();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void GenerateIds()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added);
            foreach (var entry in entries)
            {
                if(entry.Entity is Dish)
                {
                    var dish = (Dish)entry.Entity;
                    if (string.IsNullOrEmpty(dish.DishID))
                    {
                        dish.DishID = IdGenerator.GenerateDishId();
                    }
                }
            }
        }

    }
}
