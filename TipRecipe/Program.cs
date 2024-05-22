using TipRecipe.Configuration;
using TipRecipe.DbContexts;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

ConfigWebApplication.LogConfig(builder);
ConfigWebApplication.AddServices(builder);

var app = builder.Build();

if (app.Environment.IsDevelopment()){
    app.UseExceptionHandler("/error-development");
}
else{
    app.UseExceptionHandler("/error");
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();


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


