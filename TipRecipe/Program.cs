using TipRecipe.Configuration;
using TipRecipe.DbContexts;
using Microsoft.EntityFrameworkCore;



var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

ConfigWebApplication.AddServices(builder);


var app = builder.Build();


if (app.Environment.IsDevelopment()){
    app.UseExceptionHandler("/error-development");
}
else{
    app.UseExceptionHandler("/error");
}


app.UseHttpsRedirection();
app.MapControllers();
app.Run();

