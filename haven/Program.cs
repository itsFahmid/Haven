using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Haven.Data;
using Haven.Models;
using Haven.Services;
using Haven.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

// Register Microsoft SQL Server Entity Framework Core Database
builder.Services.AddDbContext<HavenDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Security & Authentication Services
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Configure Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "Haven.AuthCookie";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(14);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

// Configure Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Ensure Database is Created and Migrations Applied Automatically
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<HavenDbContext>();
        db.Database.Migrate();

        var hasher = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.IPasswordHasher<User>>();

        // 1. Seed / Ensure Admin
        var admin = db.Users.FirstOrDefault(u => u.Email == "admin@haven.org");
        if (admin == null)
        {
            admin = new User
            {
                FullName = "HAVEN Chief Admin",
                Email = "admin@haven.org",
                Role = "Admin",
                UserType = "Individual",
                Age = 30,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            admin.PasswordHash = hasher.HashPassword(admin, "Admin123!");
            db.Users.Add(admin);
        }
        else
        {
            admin.Role = "Admin";
            admin.IsActive = true;
            admin.PasswordHash = hasher.HashPassword(admin, "Admin123!");
        }
        db.SaveChanges();

        // 2. Seed / Ensure Therapist
        var therapist = db.Users.FirstOrDefault(u => u.Email == "therapist@haven.org");
        if (therapist == null)
        {
            therapist = new User
            {
                FullName = "Dr. Anika Rahman (Child & Clinical Psychologist)",
                Email = "therapist@haven.org",
                Role = "Professional",
                UserType = "Individual",
                Age = 34,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            therapist.PasswordHash = hasher.HashPassword(therapist, "Therapist123!");
            db.Users.Add(therapist);
        }
        else
        {
            therapist.Role = "Professional";
            therapist.IsActive = true;
            therapist.PasswordHash = hasher.HashPassword(therapist, "Therapist123!");
        }
        db.SaveChanges();

        // 3. Seed / Ensure Default User
        var defaultUser = db.Users.FirstOrDefault(u => u.Email == "user@haven.org");
        if (defaultUser == null)
        {
            defaultUser = new User
            {
                FullName = "Tanvir Ahmed",
                Email = "user@haven.org",
                Role = "User",
                UserType = "Individual",
                Age = 19,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            defaultUser.PasswordHash = hasher.HashPassword(defaultUser, "User123!");
            db.Users.Add(defaultUser);
        }
        else
        {
            defaultUser.PasswordHash = hasher.HashPassword(defaultUser, "User123!");
        }
        db.SaveChanges();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while applying SQL Server database migrations or seeding default accounts.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();

app.UseRouting();
app.UseSession();

// Authentication MUST be before Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapHub<HotlineHub>("/hubs/hotline");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
