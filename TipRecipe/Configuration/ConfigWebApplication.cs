using TipRecipe.Entities;
using TipRecipe.Helper;
using TipRecipe.Interfaces;
using TipRecipe.Models.Dto;

namespace TipRecipe.Configuration
{
    public class ConfigWebApplication
    {
        public static void AddServices(WebApplicationBuilder webApplication)
        {
            webApplication.Services.AddControllers(options =>
            {
                options.ReturnHttpNotAcceptable = true;
            }).AddXmlDataContractSerializerFormatters();

            webApplication.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            webApplication.Services.AddProblemDetails();
            webApplication.Services.AddSingleton<ITranslateMapper<Dish, DishDto>, DishTranslateMapper>();
        }
    }
}
