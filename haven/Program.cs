using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Haven.Data;
using Haven.Models;
using Haven.Services;
using Haven.Hubs;

// Enable legacy timestamp behavior for PostgreSQL/Npgsql to handle unzoned DateTime gracefully
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Disable file watchers for Linux container environment stability (prevents inotify limit 128 crash)
builder.Configuration.Sources.Clear();
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false);
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>(optional: true);
}
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

// Register Gemini AI Services
builder.Services.AddHttpClient<ICrisisAiService, GeminiAiService>();

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
                if (db.Database.IsSqlite())
                {
                    try { db.Database.ExecuteSqlRaw(@"CREATE TABLE IF NOT EXISTS ""Bookings"" (""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, ""UserId"" INTEGER NOT NULL, ""TherapistId"" INTEGER NOT NULL, ""BookingDate"" TEXT NOT NULL, ""TimeSlot"" TEXT NOT NULL, ""CommunicationMode"" TEXT NOT NULL, ""Notes"" TEXT NULL, ""Status"" INTEGER NOT NULL DEFAULT 0, ""BookingReference"" TEXT NOT NULL DEFAULT '', ""FeeBDT"" TEXT NOT NULL DEFAULT '0', ""CreatedAt"" TEXT NOT NULL, ""UpdatedAt"" TEXT NULL);"); } catch { }
                    try { db.Database.ExecuteSqlRaw("ALTER TABLE ProfessionalProfiles ADD COLUMN ExperienceYears INTEGER NOT NULL DEFAULT 5;"); } catch { }
                    try { db.Database.ExecuteSqlRaw("ALTER TABLE ProfessionalProfiles ADD COLUMN YearsOfExperience INTEGER NOT NULL DEFAULT 0;"); } catch { }
                    try { db.Database.ExecuteSqlRaw("ALTER TABLE ProfessionalProfiles ADD COLUMN ConsultationTime TEXT NULL;"); } catch { }
                    try { db.Database.ExecuteSqlRaw("ALTER TABLE ProfessionalProfiles ADD COLUMN Bio TEXT NULL;"); } catch { }
                    try { db.Database.ExecuteSqlRaw("ALTER TABLE ProfessionalProfiles ADD COLUMN Qualifications TEXT NULL;"); } catch { }
                    try { db.Database.ExecuteSqlRaw("ALTER TABLE ProfessionalProfiles ADD COLUMN HospitalAffiliation TEXT NULL;"); } catch { }
                    try { db.Database.ExecuteSqlRaw("ALTER TABLE ProfessionalProfiles ADD COLUMN Rating REAL NOT NULL DEFAULT 4.95;"); } catch { }
                    try { db.Database.ExecuteSqlRaw("ALTER TABLE ProfessionalProfiles ADD COLUMN ReviewCount INTEGER NOT NULL DEFAULT 120;"); } catch { }
                }
            }
        }
        catch (Exception migEx)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogWarning(migEx, "Database migration fallback triggered.");
            try { db.Database.EnsureCreated(); } catch { }
            if (db.Database.IsSqlite())
            {
                try { db.Database.ExecuteSqlRaw(@"CREATE TABLE IF NOT EXISTS ""Bookings"" (""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, ""UserId"" INTEGER NOT NULL, ""TherapistId"" INTEGER NOT NULL, ""BookingDate"" TEXT NOT NULL, ""TimeSlot"" TEXT NOT NULL, ""CommunicationMode"" TEXT NOT NULL, ""Notes"" TEXT NULL, ""Status"" INTEGER NOT NULL DEFAULT 0, ""BookingReference"" TEXT NOT NULL DEFAULT '', ""FeeBDT"" TEXT NOT NULL DEFAULT '0', ""CreatedAt"" TEXT NOT NULL, ""UpdatedAt"" TEXT NULL);"); } catch { }
                try { db.Database.ExecuteSqlRaw("ALTER TABLE ProfessionalProfiles ADD COLUMN ExperienceYears INTEGER NOT NULL DEFAULT 5;"); } catch { }
                try { db.Database.ExecuteSqlRaw("ALTER TABLE ProfessionalProfiles ADD COLUMN YearsOfExperience INTEGER NOT NULL DEFAULT 0;"); } catch { }
                try { db.Database.ExecuteSqlRaw("ALTER TABLE ProfessionalProfiles ADD COLUMN ConsultationTime TEXT NULL;"); } catch { }
                try { db.Database.ExecuteSqlRaw("ALTER TABLE ProfessionalProfiles ADD COLUMN Bio TEXT NULL;"); } catch { }
                try { db.Database.ExecuteSqlRaw("ALTER TABLE ProfessionalProfiles ADD COLUMN Qualifications TEXT NULL;"); } catch { }
                try { db.Database.ExecuteSqlRaw("ALTER TABLE ProfessionalProfiles ADD COLUMN HospitalAffiliation TEXT NULL;"); } catch { }
                try { db.Database.ExecuteSqlRaw("ALTER TABLE ProfessionalProfiles ADD COLUMN Rating REAL NOT NULL DEFAULT 4.95;"); } catch { }
                try { db.Database.ExecuteSqlRaw("ALTER TABLE ProfessionalProfiles ADD COLUMN ReviewCount INTEGER NOT NULL DEFAULT 120;"); } catch { }
            }
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
                FullName = "Dr. Anika Rahman",
                Email = "therapist@haven.org",
                Role = "Professional",
                UserType = "Individual",
                Age = 34,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            therapist.PasswordHash = hasher.HashPassword(therapist, "Therapist123!");
            db.Users.Add(therapist);
            db.SaveChanges();
        }
        else
        {
            therapist.Role = "Professional";
            therapist.IsActive = true;
            if (string.IsNullOrEmpty(therapist.PasswordHash))
            {
                therapist.PasswordHash = hasher.HashPassword(therapist, "Therapist123!");
            }
            db.SaveChanges();
        }

        var therapistProfile = db.ProfessionalProfiles.FirstOrDefault(p => p.UserId == therapist.Id);
        if (therapistProfile == null)
        {
            therapistProfile = new ProfessionalProfile
            {
                UserId = therapist.Id,
                TitleEn = "Dr. Anika Rahman, MS, MPhil (Clinical Psychology)",
                TitleBn = "ডাঃ আনিকা রহমান, এমএস, এমফিল (ক্লিনিক্যাল সাইকোলজি)",
                Specialty = "Child & Adolescent Trauma, CBT",
                LicenseNo = "BMDC Reg: A-84920",
                HourlyRateBDT = 600,
                YearsOfExperience = 8,
                ExperienceYears = 8,
                ConsultationTime = "Sat - Thu: 04:00 PM - 08:00 PM",
                Bio = "কিশোর-কিশোরী ও তরুণদের মানসিক স্বাস্থ্য, ট্রমা ও পরীক্ষার চাপ নিরসনে আন্তর্জাতিক স্ট্যান্ডার্ড সিবিটি ও সায়েন্টিফিক কাউন্সেলিং সেবা প্রদান করেন।",
                Qualifications = "MBBS (DMC), MPhil Clinical Psychology (BSMMU)",
                HospitalAffiliation = "National Institute of Mental Health (NIMH), Dhaka",
                Rating = 4.98,
                ReviewCount = 145,
                ApprovalStatus = "Approved",
                IsBmdcVerified = true,
                SubmittedAt = DateTime.UtcNow,
                VerifiedAt = DateTime.UtcNow
            };
            db.ProfessionalProfiles.Add(therapistProfile);
            db.SaveChanges();
        }

        // 3. Seed / Ensure Additional Verified Specialist (Dr. Samira Tasneem)
        var samira = db.Users.FirstOrDefault(u => u.Email == "dr.samira@haven.org");
        if (samira == null)
        {
            samira = new User
            {
                FullName = "Dr. Samira Tasneem",
                Email = "dr.samira@haven.org",
                Role = "Professional",
                UserType = "Individual",
                Age = 36,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            samira.PasswordHash = hasher.HashPassword(samira, "Therapist123!");
            db.Users.Add(samira);
            db.SaveChanges();

            var samiraProfile = new ProfessionalProfile
            {
                UserId = samira.Id,
                TitleEn = "Senior Psychiatrist & Cyber Harassment Specialist",
                TitleBn = "সিনিয়র সাইকিয়াট্রিস্ট ও সাইবার ট্রমা বিশেষজ্ঞ",
                Specialty = "Cyber Harassment Recovery, Depression & Panic",
                LicenseNo = "BMDC Reg: A-74291",
                ApprovalStatus = "Approved",
                IsBmdcVerified = true,
                HourlyRateBDT = 700,
                ExperienceYears = 9,
                YearsOfExperience = 9,
                Rating = 4.96,
                ReviewCount = 186,
                Bio = "সাইবার বুলিং, অনলাইন ব্ল্যাকমেইলিং ও সোশ্যাল মিডিয়া বিষণ্ণতায় আক্রান্ত তরুণদের তাৎক্ষণিক মানসিক প্রাথমিক চিকিৎসা ও দীর্ঘমেয়াদী থেরাপি স্পেশালিস্ট।",
                Qualifications = "MBBS (CMC), MD Psychiatry (BSMMU)",
                HospitalAffiliation = "Dhaka Medical College Hospital",
                SubmittedAt = DateTime.UtcNow,
                VerifiedAt = DateTime.UtcNow
            };
            db.ProfessionalProfiles.Add(samiraProfile);
            db.SaveChanges();
        }

        // 4. Seed / Ensure Unverified/Pending Therapist
        var pendingDoc = db.Users.FirstOrDefault(u => u.Email == "pending.doc@haven.org");
        if (pendingDoc == null)
        {
            pendingDoc = new User
            {
                FullName = "Dr. Rafiqul Islam (Pending Verification)",
                Email = "pending.doc@haven.org",
                Role = "Professional",
                UserType = "Individual",
                Age = 40,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            pendingDoc.PasswordHash = hasher.HashPassword(pendingDoc, "Therapist123!");
            db.Users.Add(pendingDoc);
            db.SaveChanges();

            var pendingProfile = new ProfessionalProfile
            {
                UserId = pendingDoc.Id,
                TitleEn = "General Mental Health Counselor (Pending Review)",
                TitleBn = "জেনারেল কাউন্সেলর (যাচাইকরণাধীন)",
                Specialty = "General Counseling",
                LicenseNo = "BMDC Reg: A-99999",
                ApprovalStatus = "Pending",
                IsBmdcVerified = false,
                HourlyRateBDT = 400,
                ExperienceYears = 2,
                YearsOfExperience = 2,
                Rating = 4.5,
                ReviewCount = 0,
                Bio = "Pending admin verification profile.",
                Qualifications = "MBBS",
                HospitalAffiliation = "Private Practice",
                SubmittedAt = DateTime.UtcNow
            };
            db.ProfessionalProfiles.Add(pendingProfile);
            db.SaveChanges();
        }

        // 5. Seed / Ensure Default User
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

        // 6. Seed Sample Bookings
        if (!db.Bookings.Any() && therapistProfile != null && defaultUser != null)
        {
            db.Bookings.AddRange(
                new Booking
                {
                    UserId = defaultUser.Id,
                    TherapistId = therapistProfile.Id,
                    BookingDate = DateTime.UtcNow.AddDays(2).Date,
                    TimeSlot = "04:30 PM - 05:30 PM",
                    CommunicationMode = "Online Video",
                    Notes = "কিশোর বয়সের তীব্র পরীক্ষার মানসিক চাপ ও উদ্বেগ নিরসনে আলোচনা করতে চাই।",
                    Status = BookingStatus.Pending,
                    BookingReference = "HVN-BK-10492",
                    FeeBDT = 600,
                    CreatedAt = DateTime.UtcNow.AddHours(-3)
                },
                new Booking
                {
                    UserId = defaultUser.Id,
                    TherapistId = therapistProfile.Id,
                    BookingDate = DateTime.UtcNow.AddDays(-1).Date,
                    TimeSlot = "07:00 PM - 08:00 PM",
                    CommunicationMode = "Confidential Audio",
                    Notes = "প্যানিক অ্যাটাক নিয়ন্ত্রণ ও ৫-৪-৩-২-১ গ্রাউন্ডিং টেকনিক সেশন।",
                    Status = BookingStatus.Approved,
                    BookingReference = "HVN-BK-10381",
                    FeeBDT = 600,
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1)
                }
            );
            db.SaveChanges();
        }
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
            if (string.IsNullOrWhiteSpace(database)) database = "neondb";

            return $"Host={host};Port={port};Database={database};Username={user};Password={password};SSL Mode=Require;Trust Server Certificate=true";
        }
        catch
        {
            return url;
        }
    }
    return url;
}
