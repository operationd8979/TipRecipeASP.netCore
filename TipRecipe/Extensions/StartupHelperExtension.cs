using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog.Events;
using Serilog;
using TipRecipe.DbContexts;
using TipRecipe.Entities;
using TipRecipe.Helper;
using TipRecipe.Interfaces;
using TipRecipe.Models.Dto;
using TipRecipe.Repositorys;
using TipRecipe.Services;
using System.Security.Claims;
using TipRecipe.Filters;
using Amazon;
using Amazon.Runtime;
using Amazon.SecretsManager.Model;
using Amazon.SecretsManager;
using System.Text.Json;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.DataProtection;
using Azure.Core;
namespace TipRecipe.Extensions
{
    public static class StartupHelperExtension
    {
        public static WebApplicationBuilder LogConfig(this WebApplicationBuilder builder)
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
            builder.Host.UseSerilog();
            return builder;
        }

        public static async Task GetSecretKeyFromAWS(WebApplicationBuilder builder)
        {
            string secretName = builder.Configuration["AWS:SecretName"]!;

            //BasicAWSCredentials awsCreds = new BasicAWSCredentials(builder.Configuration["AWS:AccessKey"], builder.Configuration["AWS:SecretKey"]);

            IAmazonSecretsManager client = new AmazonSecretsManagerClient(
                //awsCreds,
                RegionEndpoint.GetBySystemName(builder.Configuration["AWS:Region"]));

            GetSecretValueRequest request = new GetSecretValueRequest
            {
                SecretId = secretName,
                VersionStage = "AWSCURRENT",
            };
            GetSecretValueResponse response;
            try
            {
                response = await client.GetSecretValueAsync(request);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Get secrect key fail");
                throw;
            }

            var secret = response.SecretString;

            //var secretStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(secret));
            //builder.Configuration.AddJsonStream(secretStream);

            //add prefix ConnectionStrings to secret key
            var configDict = new Dictionary<string, string>();
            using (var jsonDoc = JsonDocument.Parse(secret))
            {
                foreach (var property in jsonDoc.RootElement.EnumerateObject())
                {
                    configDict[$"ConnectionStrings:{property.Name}"] = property.Value.GetString()!;
                }
            }

            // Add the configuration to ConfigurationBuilder
            builder.Configuration.AddInMemoryCollection(configDict!);
        }

        public static async Task GetSecretKeyFromAzure(WebApplicationBuilder builder)
        {
            
            string keyVaultName = builder.Configuration["Azure:KeyVault:Name"]!;
            var keyVaultUri = new Uri($"https://{keyVaultName}.vault.azure.net/");

            TokenCredential credential;
            if (builder.Environment.IsDevelopment())
            {
                string tenantId = builder.Configuration["Azure:Ad:TenantId"]!;
                string clientId = builder.Configuration["Azure:Ad:ClientId"]!;
                string clientSecret = builder.Configuration["Azure:Ad:ClientSecret"]!;
                credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
            }
            else
            {
                credential = new DefaultAzureCredential();
            }

            var secretClient = new SecretClient(keyVaultUri, credential);

            var secretNames = new[] { "AzureStorage", "AzureSQL" };
            var secretTasks = new Task<Azure.Response<KeyVaultSecret>>[secretNames.Length];
            for (int i = 0; i < secretNames.Length; i++)
            {
                secretTasks[i] = secretClient.GetSecretAsync(secretNames[i]);
            }

            var secrets = await Task.WhenAll(secretTasks);

            secrets.ToList().ForEach(secret =>
            {
                string secretName = secret.Value.Name;
                string secretValue = secret.Value.Value;
                var configDict = new Dictionary<string, string>();
                configDict[$"ConnectionStrings:{secretName}"] = secretValue;
                builder.Configuration.AddInMemoryCollection(configDict!);
            });

            // Add Azure Key Vault secrets to configuration
            //builder.Configuration.AddAzureKeyVault(keyVaultUri, credential);
        }

        public static async Task<WebApplicationBuilder> ConnectDbStorage(this WebApplicationBuilder builder)
        {
            //get secret key from cloud
            //await GetSecretKeyFromAWS(builder);
            await GetSecretKeyFromAzure(builder);

            //azure blob service
            builder.Services.AddSingleton(
                new AzureBlobService(builder.Configuration.GetConnectionString("AzureStorage")!.ToString()));

            //add sql server
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("AzureSQL")));
            //builder.Services.AddDbContext<ApplicationDbContext>(options =>
            //    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            return builder;
        }

        public static WebApplicationBuilder AddServices(this WebApplicationBuilder builder)
        {
            //authentication Authorization               
            builder.Services.AddAuthentication(options =>
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
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Convert.FromBase64String(builder.Configuration["Jwt:Key"]
                        ?? throw new ArgumentNullException(nameof(options))))
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var token = context.Request.Cookies["jwt"];
                        if (!string.IsNullOrEmpty(token))
                        {
                            context.Token = token;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            builder.Services.AddAuthorization(
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
            builder.Services.AddControllers(options =>
            {
                options.SuppressAsyncSuffixInActionNames = false;
                options.InputFormatters.Insert(0, MyJpif.GetJsonPatchInputFormatter());
                options.ReturnHttpNotAcceptable = true;
            }).AddXmlDataContractSerializerFormatters();

            builder.Services.AddProblemDetails();

            //add automapper profiles
            builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            //add custom mapper dto
            builder.Services.AddSingleton<ITranslateMapper<Dish, DishDto>, DishTranslateMapper>();

            //add caching file service
            builder.Services.AddSingleton<CachingFileService>(provider =>
            {
                var logger = provider.GetRequiredService<ILogger<CachingFileService>>();
                return new CachingFileService(logger);
            });
            builder.Services.AddSingleton<CachedRatingScoreService>();

            //add repositories and services
            builder.Services.AddScoped<UserManager>();
            builder.Services.AddScoped<IDishRepository, DishRepository>();
            builder.Services.AddScoped<IIngredientRepository, IngredientRepository>();
            builder.Services.AddScoped<ITypeDishRepository, TypeDishRepository>();
            builder.Services.AddScoped<DishService>();

            //add side middleware service
            builder.Services.AddSingleton<AddInfoServerMiddleware>();

            //add background service
            builder.Services.AddHostedService<DishBackgroundService>();

            
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAngularDev",
                    builder =>
                    {
                        builder.WithOrigins("http://localhost:4200")
                               .AllowAnyHeader()
                               .AllowAnyMethod()
                               .AllowCredentials();
                    });
            });


            return builder;
        }

        public static WebApplication UseMiddleware(this WebApplication app)
        {
            //app.UseMiddleware<MutithreadMiddleware>();
            //app.UseMiddleware<DelayMiddleware>();
            if (app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/error-development");
            }
            else
            {
                app.UseExceptionHandler("/error");
            }
            app.UseCors("AllowAngularDev");
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            return app;
        }


    }


}
