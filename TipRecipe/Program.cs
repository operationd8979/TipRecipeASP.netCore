using TipRecipe.Extensions;
using Serilog;
using TipRecipe;

var builder = WebApplication.CreateBuilder(args);


builder.LogConfig();
await builder.ConnectDbContext();
builder.AddServices();

var app = builder.Build();

app.UseMiddleware();


try
{
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}


