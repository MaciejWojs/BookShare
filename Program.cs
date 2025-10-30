using BookShare.Data;
using BookShare.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore; // <-- dodane: zapewnia rozszerzenia dla IdentityBuilder
using dotenv.net;

DotEnv.Load();

var builder = WebApplication.CreateBuilder(args);

var host = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
var port = 5432;
var name = Environment.GetEnvironmentVariable("POSTGRES_DB");
var user = Environment.GetEnvironmentVariable("POSTGRES_USER");
var pass = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");

var connectionString = $"Host={host};Port={port};Database={name};Username={user};Password={pass}";

// Console.WriteLine("Using connection string: " + connectionString);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(
    options => options.UseNpgsql(connectionString));


builder.Services.AddIdentity<User, IdentityRole>(options => {
    // konfiguracja polityk haseł itp. (dostosuj)
    options.SignIn.RequireConfirmedAccount = false; // brak wymogu potwierdzenia e-mail 
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
})
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultUI()               // <-- zapewnia Razor Pages Identity (Login/Register)
    .AddDefaultTokenProviders();


builder.Services.AddRazorPages();
var app = builder.Build();

await using var scope = app.Services.CreateAsyncScope();
var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
var canConnect = await db.Database.CanConnectAsync();
app.Logger.LogInformation("Can connect to database: {CanConnect}", canConnect);
if (canConnect) {


    try {
        app.Logger.LogInformation("Can connect to database: {CanConnect}", canConnect);

        // Automatyczne zastosowanie migracji tylko w Development.
        // if (app.Environment.IsDevelopment())
        // {
        // }
        await db.Database.MigrateAsync();
        app.Logger.LogInformation("Applied pending EF Core migrations.");
        // else
        // {
        //     app.Logger.LogInformation("Skipped automatic migrations in non-development environment. Create/apply migrations manually: 'dotnet ef migrations add <Name>' and 'dotnet ef database update'.");
        // }
    }
    catch (Exception ex) {
        app.Logger.LogError(ex, "Database migration/connect failed.");
        // W środowisku produkcyjnym rozważ zakończenie startu aplikacji:
        // throw;
    }

}
else {
    app.Logger.LogError("Cannot connect to database. Exiting application.");
    Environment.Exit(0);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication(); // <-- ważne
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();
app.MapRazorPages();

app.Run();
