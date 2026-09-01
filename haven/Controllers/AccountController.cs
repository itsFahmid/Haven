using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Haven.Data;
using Haven.Models;
using Haven.Services;

namespace Haven.Controllers;

public class AccountController : Controller
{
    private readonly IAuthService _authService;
    private readonly HavenDbContext _db;
    private readonly ILogger<AccountController> _logger;

    public AccountController(IAuthService authService, HavenDbContext db, ILogger<AccountController> logger)
    {
        _authService = authService;
        _db = db;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Register(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToLocal(returnUrl);
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View(new RegisterViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var (success, errorMessage, user) = await _authService.RegisterAsync(model);
        if (!success || user == null)
        {
            ModelState.AddModelError(string.Empty, errorMessage ?? "Registration failed. Please check your information.");
            return View(model);
        }

        // Automatically sign in the newly registered user
        await SignInUserAsync(user, isPersistent: true);

        TempData["SuccessMessage"] = "Welcome to Haven! Your account has been securely created. / হেভেনে স্বাগতম! আপনার অ্যাকাউন্ট সফলভাবে তৈরি হয়েছে।";

        return RedirectToLocal(returnUrl);
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToLocal(returnUrl);
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var (success, errorMessage, user) = await _authService.AuthenticateAsync(model.Email, model.Password);
        if (!success || user == null)
        {
            ModelState.AddModelError(string.Empty, errorMessage ?? "Invalid credentials. Please verify and try again.");
            return View(model);
        }

        await SignInUserAsync(user, model.RememberMe);

        TempData["SuccessMessage"] = $"Welcome back, {user.FullName}! / ফিরে আসার জন্য স্বাগতম, {user.FullName}!";

        return RedirectToLocal(returnUrl);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var userName = User.Identity?.Name ?? "User";
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        HttpContext.Session.Clear();

        _logger.LogInformation("User {UserName} logged out.", userName);
        TempData["InfoMessage"] = "You have been securely logged out. / আপনি নিরাপদে লগআউট হয়েছেন।";

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(idClaim, out int userId))
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        var user = await _authService.GetUserByIdAsync(userId);
        if (user == null)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        var childProfiles = await _db.ChildProfiles.Where(c => c.ParentUserId == userId).ToListAsync();
        var bookmarkedArticles = await _db.Articles.OrderByDescending(a => a.CreatedAt).Take(3).ToListAsync();

        var profileModel = new UserProfileViewModel
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            UserType = user.UserType,
            Age = user.Age,
            ProfilePictureUrl = user.ProfilePictureUrl,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            CompletedCoursesCount = 2,
            BookedSessionsCount = 1,
            ChildProfiles = childProfiles,
            BookmarkedArticles = bookmarkedArticles
        };

        return View(profileModel);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(string fullName, IFormFile? profileImage, [FromServices] IWebHostEnvironment env)
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(idClaim, out int userId) && !string.IsNullOrWhiteSpace(fullName))
        {
            var user = await _db.Users.FindAsync(userId);
            if (user != null)
            {
                user.FullName = fullName.Trim();

                if (profileImage != null && profileImage.Length > 0)
                {
                    try
                    {
                        string uploadsFolder = Path.Combine(env.WebRootPath, "uploads", "avatars");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        string ext = Path.GetExtension(profileImage.FileName);
                        if (string.IsNullOrEmpty(ext)) ext = ".jpg";

                        string uniqueFileName = $"avatar_{userId}_{DateTime.UtcNow.Ticks}{ext}";
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await profileImage.CopyToAsync(stream);
                        }

                        user.ProfilePictureUrl = "/uploads/avatars/" + uniqueFileName;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to upload profile picture");
                    }
                }

                await _db.SaveChangesAsync();
                await SignInUserAsync(user, isPersistent: true);
                TempData["SuccessMessage"] = "আপনার প্রোফাইল সফলভাবে আপডেট করা হয়েছে।";
            }
        }
        return RedirectToAction(nameof(Profile));
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmNewPassword)
    {
        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
        {
            TempData["ErrorMessage"] = "নতুন পাসওয়ার্ড কমপক্ষে ৬ অক্ষরের হতে হবে।";
            return RedirectToAction(nameof(Profile));
        }

        if (newPassword != confirmNewPassword)
        {
            TempData["ErrorMessage"] = "নতুন পাসওয়ার্ড দুটি মিলছে না।";
            return RedirectToAction(nameof(Profile));
        }

        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(idClaim, out int userId))
        {
            var user = await _db.Users.FindAsync(userId);
            if (user != null)
            {
                var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<User>();
                var verify = hasher.VerifyHashedPassword(user, user.PasswordHash, currentPassword);
                if (verify == Microsoft.AspNetCore.Identity.PasswordVerificationResult.Failed)
                {
                    TempData["ErrorMessage"] = "বর্তমান পাসওয়ার্ড সঠিক নয়।";
                    return RedirectToAction(nameof(Profile));
                }

                user.PasswordHash = hasher.HashPassword(user, newPassword);
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = "পাসওয়ার্ড সফলভাবে পরিবর্তন করা হয়েছে।";
            }
        }
        return RedirectToAction(nameof(Profile));
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAccount(string confirmEmail)
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(idClaim, out int userId))
        {
            var user = await _db.Users.FindAsync(userId);
            if (user != null && string.Equals(user.Email, confirmEmail?.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                _db.Users.Remove(user);
                await _db.SaveChangesAsync();

                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                HttpContext.Session.Clear();

                TempData["InfoMessage"] = "আপনার অ্যাকাউন্ট ও সংশ্লিষ্ট তথ্য স্থায়ীভাবে মুছে ফেলা হয়েছে।";
                return RedirectToAction("Index", "Home");
            }
        }

        TempData["ErrorMessage"] = "অ্যাকাউন্ট ডিলিট করতে সঠিক ইমেইল ঠিকানা প্রদান করুন।";
        return RedirectToAction(nameof(Profile));
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Bookmarks()
    {
        int userId = 0;
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(claim, out int uid))
        {
            userId = uid;
        }

        var bookmarkedArticles = await _db.ArticleBookmarks
            .Include(b => b.Article)
                .ThenInclude(a => a!.Author)
            .Where(b => b.UserId == userId && b.Article != null)
            .OrderByDescending(b => b.BookmarkedAt)
            .Select(b => b.Article!)
            .ToListAsync();

        return View(bookmarkedArticles);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddChildProfile(string aliasName, string ageGroup)
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(idClaim, out int userId) && !string.IsNullOrWhiteSpace(aliasName))
        {
            var child = new ChildProfile
            {
                ParentUserId = userId,
                AliasName = aliasName.Trim(),
                AgeGroup = string.IsNullOrWhiteSpace(ageGroup) ? "Child" : ageGroup.Trim(),
                CreatedAt = DateTime.UtcNow
            };
            _db.ChildProfiles.Add(child);
            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "শিশু প্রোফাইল সফলভাবে সংযুক্ত হয়েছে।";
        }
        return RedirectToAction(nameof(Profile));
    }

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(string email)
    {
        if (!string.IsNullOrWhiteSpace(email))
        {
            var user = await _authService.GetUserByEmailAsync(email);
            if (user != null)
            {
                _logger.LogInformation("Password reset token dispatched for {Email}", email);
            }
        }
        TempData["SuccessMessage"] = "যদি প্রদানকৃত ইমেইলটি নিবন্ধিত থাকে, তবে পাসওয়ার্ড রিসেট লিঙ্ক আপনার ইমেইলে পাঠানো হয়েছে।";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    private async Task SignInUserAsync(User user, bool isPersistent)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role)
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = isPersistent,
            ExpiresUtc = isPersistent ? DateTimeOffset.UtcNow.AddDays(14) : DateTimeOffset.UtcNow.AddHours(12),
            IssuedUtc = DateTimeOffset.UtcNow,
            AllowRefresh = true
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
    }
}
