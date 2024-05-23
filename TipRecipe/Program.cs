using TipRecipe.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.LogConfig();
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


