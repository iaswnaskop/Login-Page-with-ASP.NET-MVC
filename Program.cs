using LoginPage.Context;
using LoginPage.Services.Login;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<DataContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddScoped<ILoginService, LoginService>();

// builder.WebHost.ConfigureKestrel(options =>
// {
//     options.ListenAnyIP(9090); 
// });
// builder.Services.AddHealthChecks()
//     .AddNpgSql(connectionString: builder.Configuration.GetConnectionString("DefaultConnection"));
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}



app.UseHttpsRedirection();
app.UseRouting();
try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<DataContext>();
    db.Database.Migrate();
    Console.WriteLine("Database migrated successfully.");
}
catch (Exception ex)
{
    Console.WriteLine("Migration failed: " + ex.Message);
    throw;
}

app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/LoginView/LoginIndex"))
    {
        Console.WriteLine($"Request Path: {context.Request.Path}");

        var token = context.Request.Query["token"].FirstOrDefault() ??
                    context.Request.Cookies["token"] ?? 
                    context.Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "");

        if (!string.IsNullOrEmpty(token))
        {
            Console.WriteLine("Token found");
            context.Request.Headers["Authorization"] = $"Bearer {token}";
        }
    }
    
    await next();
});

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();