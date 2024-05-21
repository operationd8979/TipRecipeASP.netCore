using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using TipRecipe.DbContexts;
using TipRecipe.Entities;
using TipRecipe.Helper;
using TipRecipe.Interfaces;
using TipRecipe.Models.Dto;
using TipRecipe.Repositorys;
using TipRecipe.Services;

namespace TipRecipe.Configuration
{
    public class ConfigWebApplication
    {
        public static void AddServices(WebApplicationBuilder webApplication)
        {
            //add dbcontext
            webApplication.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(webApplication.Configuration.GetConnectionString("DefaultConnection")));

            //not use SuppressAsyncSuffixInActionNames
            webApplication.Services.AddMvc(options =>
            {
                options.SuppressAsyncSuffixInActionNames = false;
            });

            //add json patch format
            //add xml format
            webApplication.Services.AddControllers(options =>
            {
                options.InputFormatters.Insert(0, MyJPIF.GetJsonPatchInputFormatter());
                options.ReturnHttpNotAcceptable = true;
            }).AddXmlDataContractSerializerFormatters();

            //add automapper profiles
            webApplication.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            webApplication.Services.AddProblemDetails();

            //add caching file service
            webApplication.Services.AddSingleton<CachingFileService>(provider =>
            {
                var logger = provider.GetRequiredService<ILogger<CachingFileService>>();
                return new CachingFileService("Caches/cachefile.json", logger);
            });

            //add custom mapper dto
            webApplication.Services.AddSingleton<ITranslateMapper<Dish, DishDto>, DishTranslateMapper>();
            
            //add repositories and services
            webApplication.Services.AddScoped<IDishRepository, DishRepository>();
            webApplication.Services.AddScoped<IIngredientRepository, IngredientRepository>();
            webApplication.Services.AddScoped<ITypeDishRepository, TypeDishRepository>();
            webApplication.Services.AddScoped<DishService>();
        }

        public static void LogConfig(WebApplicationBuilder webApplication)
        {
            Log.Logger = new LoggerConfiguration()
             .Enrich.FromLogContext()
             .WriteTo.Console()
             .WriteTo.File(
                 path: "Logs/log-.txt",
                 rollingInterval: RollingInterval.Day,
                 outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] {Message}{NewLine}{Exception}")
             .MinimumLevel.Information()
             .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
             .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
             .CreateLogger();
            Log.Information("Starting web host");
            webApplication.Host.UseSerilog();
        }


    }
}
