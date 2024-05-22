using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using System.Security.Claims;
using System.Text;
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

            //authentication Authorization               
            webApplication.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = webApplication.Configuration["Jwt:Issuer"],
                    ValidAudience = webApplication.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Convert.FromBase64String(webApplication.Configuration["Jwt:Key"]
                        ?? throw new ArgumentNullException("JWT config")))
                };
            });
            //.AddCookie(options =>
            //{
            //    options.Cookie.Name = "jwt";
            //    options.Cookie.HttpOnly = true;
            //    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            //});

            webApplication.Services.AddAuthorization(
                options =>
                {
                    options.AddPolicy("User", policy =>
                    {
                        policy.RequireAuthenticatedUser();
                        policy.RequireAssertion(context =>
                        {
                            var roleClaims = context.User.FindAll(ClaimTypes.Role);
                            return roleClaims.Any(claim => claim.Value.Contains("USER"));
                        });
                    });
                    options.AddPolicy("Admin", policy =>
                    {
                        policy.RequireAuthenticatedUser();
                        policy.RequireAssertion(context =>
                        {
                            var roleClaims = context.User.FindAll(ClaimTypes.Role);
                            return roleClaims.Any(claim => claim.Value.Contains("ADMIN"));
                        });
                    });
                }
            );



            //not use SuppressAsyncSuffixInActionNames
            //add json patch format
            //add xml format
            webApplication.Services.AddControllers(options =>
            {
                options.SuppressAsyncSuffixInActionNames = false;
                options.InputFormatters.Insert(0, MyJPIF.GetJsonPatchInputFormatter());
                options.ReturnHttpNotAcceptable = true;
            }).AddXmlDataContractSerializerFormatters();

            webApplication.Services.AddProblemDetails();

            //add automapper profiles
            webApplication.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            //add custom mapper dto
            webApplication.Services.AddSingleton<ITranslateMapper<Dish, DishDto>, DishTranslateMapper>();

            //add caching file service
            webApplication.Services.AddSingleton<CachingFileService>(provider =>
            {
                var logger = provider.GetRequiredService<ILogger<CachingFileService>>();
                return new CachingFileService("Caches/cachefile.json", logger);
            });

            //add repositories and services
            webApplication.Services.AddScoped<UserManager>();
            webApplication.Services.AddScoped<IDishRepository, DishRepository>();
            webApplication.Services.AddScoped<IIngredientRepository, IngredientRepository>();
            webApplication.Services.AddScoped<ITypeDishRepository, TypeDishRepository>();
            webApplication.Services.AddScoped<DishService>();

            //add background service
            webApplication.Services.AddHostedService<DishBackgroundService>();
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
