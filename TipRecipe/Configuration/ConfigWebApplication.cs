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
            webApplication.Services.AddMvc(options =>
            {
                options.SuppressAsyncSuffixInActionNames = false;
            });

            webApplication.Services.AddControllers(options =>
            {
                options.InputFormatters.Insert(0, MyJPIF.GetJsonPatchInputFormatter());
                options.ReturnHttpNotAcceptable = true;
            }).AddXmlDataContractSerializerFormatters();

            webApplication.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            webApplication.Services.AddProblemDetails();

            webApplication.Services.AddSingleton<ITranslateMapper<Dish, DishDto>, DishTranslateMapper>();
            webApplication.Services.AddScoped<IDishRepository, DishRepository>();
            webApplication.Services.AddScoped<IIngredientRepository, IngredientRepository>();
            webApplication.Services.AddScoped<ITypeDishRepository, TypeDishRepository>();
            webApplication.Services.AddScoped<DishService>();
        }
    }
}
