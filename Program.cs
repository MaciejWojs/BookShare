using BookShare.Data;
using BookShare.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using dotenv.net;
using System.Linq; // <-- dodane

DotEnv.Load();

var builder = WebApplication.CreateBuilder(args);

var host = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
var port = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
var name = Environment.GetEnvironmentVariable("POSTGRES_DB");
var user = Environment.GetEnvironmentVariable("POSTGRES_USER");
var pass = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");

var connectionString = $"Host={host};Port={port};Database={name};Username={user};Password={pass}";

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddIdentity<User, IdentityRole>(options => {
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
})
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultUI()
    .AddDefaultTokenProviders();

builder.Services.AddRazorPages();

var app = builder.Build();

await using var scope = app.Services.CreateAsyncScope();
var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

Console.WriteLine("czy DEV dziala?"+ app.Environment.IsDevelopment());

try {
    if (!await db.Database.CanConnectAsync()) {
        app.Logger.LogError("Cannot connect to database. Exiting.");
        Environment.Exit(1);
    }

    await db.Database.MigrateAsync();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

    var requiredRoles = new[] { "Administrator", "Customer" };
    foreach (var r in requiredRoles) {
        if (!await roleManager.RoleExistsAsync(r)) {
            await roleManager.CreateAsync(new IdentityRole(r));
            app.Logger.LogInformation("Created role: {Role}", r);
        }
    }
    
    if (app.Environment.IsDevelopment()) {
        // Utwórz pierwszego użytkownika (admin) jeśli brak użytkowników
        if (!await userManager.Users.AnyAsync()) {
            Console.WriteLine("Tworzenie admina...");
            var admin = new User {
                UserName = "admin",
                Email = "admin@example.com",
                Role = UserRole.Administrator
            };
            var createRes = await userManager.CreateAsync(admin, "Admin123!");

            if (createRes.Succeeded) {
                Console.WriteLine("Dodawanie do roli...");
                await userManager.AddToRoleAsync(admin, "Administrator");
                app.Logger.LogInformation("Created initial admin user: {User}", admin.UserName);
            }
            else {
                app.Logger.LogWarning("Failed to create initial admin user: {Errors}", string.Join(", ", createRes.Errors.Select(e => e.Description)));
            }
        }
    }

    var users = await userManager.Users.ToListAsync();
    foreach (var u in users) {
        try {
            var target = u.Role == UserRole.Administrator ? "Administrator" : "Customer";
            if (!await userManager.IsInRoleAsync(u, target)) {
                var res = await userManager.AddToRoleAsync(u, target);
                if (!res.Succeeded)
                    app.Logger.LogWarning("Failed assigning role {Role} to {User}: {Errors}", target, u.UserName ?? u.Username, string.Join(", ", res.Errors.Select(e => e.Description)));
            }
        }
        catch (Exception ex) {
            app.Logger.LogError(ex, "Error syncing roles for user {User}", u.UserName ?? u.Username);
        }
    }
}
catch (Exception ex) {
    app.Logger.LogError(ex, "Database init failed.");
    // w razie potrzeby: throw;
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages();

app.Run();
