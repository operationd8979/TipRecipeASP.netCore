using TipRecipe.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.LogConfig();
await builder.ConnectDbStorage();
builder.AddServices();

var app = builder.Build();

app.UseMiddleware();

try
{
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}


