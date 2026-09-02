using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Haven.Data;
using Haven.Models;
using Haven.Services;
using Haven.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Disable file watchers for Linux container environment stability (prevents inotify limit 128 crash)
builder.Configuration.Sources.Clear();
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false);
builder.Configuration.AddEnvironmentVariables();

// Support dynamic port binding on cloud hosts like Render
var renderPort = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(renderPort))
{
    builder.WebHost.UseUrls($"http://*:{renderPort}");
}

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

// Register Entity Framework Core Database with automatic PostgreSQL, SQL Server, and SQLite fallback for Cloud/Docker environments
var rawConnection = Environment.GetEnvironmentVariable("DATABASE_URL")
                    ?? builder.Configuration["DATABASE_URL"] 
                    ?? builder.Configuration.GetConnectionString("DefaultConnection") 
                    ?? builder.Configuration["DefaultConnection"];

builder.Services.AddDbContext<HavenDbContext>(options =>
{
    if (!string.IsNullOrWhiteSpace(rawConnection) &&
        (rawConnection.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
         rawConnection.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase) ||
         rawConnection.Contains("Port=5432") ||
         rawConnection.Contains("User Id=") ||
         rawConnection.Contains("Username=")))
    {
        string pgConnectionString = ConvertPostgresUrlToConnectionString(rawConnection);
        options.UseNpgsql(pgConnectionString);
    }
    else if (!string.IsNullOrWhiteSpace(rawConnection) &&
             !rawConnection.Contains("SQLEXPRESS") &&
             !rawConnection.Contains("haven.db") &&
             rawConnection.Contains("Server="))
    {
        options.UseSqlServer(rawConnection);
    }
    else
    {
        string dataDir = Environment.GetEnvironmentVariable("DATA_DIR") ?? builder.Environment.ContentRootPath;
        if (!Directory.Exists(dataDir)) Directory.CreateDirectory(dataDir);
        string dbPath = Path.Combine(dataDir, "haven.db");
        options.UseSqlite($"Data Source={dbPath}");
    }
});

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
        try
        {
            if (db.Database.IsSqlServer())
            {
                db.Database.Migrate();
            }
            else
            {
                db.Database.EnsureCreated();
            }
        }
        catch (Exception migEx)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogWarning(migEx, "Database migration fallback triggered.");
            try { db.Database.EnsureCreated(); } catch { }
        }

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
            if (string.IsNullOrEmpty(admin.PasswordHash))
            {
                admin.PasswordHash = hasher.HashPassword(admin, "Admin123!");
            }
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
            if (string.IsNullOrEmpty(therapist.PasswordHash))
            {
                therapist.PasswordHash = hasher.HashPassword(therapist, "Therapist123!");
            }
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
            if (string.IsNullOrEmpty(defaultUser.PasswordHash))
            {
                defaultUser.PasswordHash = hasher.HashPassword(defaultUser, "User123!");
            }
        }
        db.SaveChanges();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while database initialization or seeding default accounts.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
});

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

static string ConvertPostgresUrlToConnectionString(string url)
{
    if (string.IsNullOrWhiteSpace(url)) return url;
    if (url.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
        url.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
    {
        try
        {
            var uri = new Uri(url);
            var userInfo = uri.UserInfo.Split(':', 2);
            var user = Uri.UnescapeDataString(userInfo[0]);
            var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";
            var host = uri.Host;
            var port = uri.Port > 0 ? uri.Port : 5432;
            var database = uri.AbsolutePath.TrimStart('/');

            return $"Host={host};Port={port};Database={database};Username={user};Password={password};SSL Mode=Prefer;Trust Server Certificate=true";
        }
        catch
        {
            return url;
        }
    }
    return url;
}
