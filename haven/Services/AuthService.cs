using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Haven.Data;
using Haven.Models;

namespace Haven.Services;

public class AuthService : IAuthService
{
    private readonly HavenDbContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        HavenDbContext context,
        IPasswordHasher<User> passwordHasher,
        ILogger<AuthService> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<(bool Success, string? ErrorMessage, User? User)> RegisterAsync(RegisterViewModel model)
    {
        try
        {
            var normalizedEmail = model.Email.Trim().ToLowerInvariant();

            // Check duplicate email
            var emailExists = await _context.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail);
            if (emailExists)
            {
                return (false, "An account with this email already exists / এই ইমেইল দিয়ে ইতিমধ্যে অ্যাকাউন্ট খোলা আছে।", null);
            }

            var user = new User
            {
                FullName = model.FullName.Trim(),
                Email = normalizedEmail,
                Role = "User",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            // Hash password securely
            user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("New user registered successfully with Email: {Email}", user.Email);
            return (true, null, user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during user registration for {Email}", model.Email);
            return (false, "An unexpected error occurred while creating your account. Please try again.", null);
        }
    }

    public async Task<(bool Success, string? ErrorMessage, User? User)> AuthenticateAsync(string email, string password)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return (false, "Email and password are required / ইমেইল ও পাসওয়ার্ড প্রদান করুন।", null);
            }

            var normalizedEmail = email.Trim().ToLowerInvariant();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);

            if (user == null)
            {
                _logger.LogWarning("Authentication failed: User not found for email {Email}", normalizedEmail);
                return (false, "Invalid email or password / ভুল ইমেইল অথবা পাসওয়ার্ড।", null);
            }

            if (!user.IsActive)
            {
                return (false, "Your account has been deactivated. Please contact Haven support.", null);
            }

            var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            if (verificationResult == PasswordVerificationResult.Failed)
            {
                _logger.LogWarning("Authentication failed: Incorrect password for user {Email}", normalizedEmail);
                return (false, "Invalid email or password / ভুল ইমেইল অথবা পাসওয়ার্ড।", null);
            }

            // If password needs rehash (security update algorithm), update it
            if (verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
            {
                user.PasswordHash = _passwordHasher.HashPassword(user, password);
                await _context.SaveChangesAsync();
            }

            await UpdateLastLoginAsync(user.Id);

            _logger.LogInformation("User logged in successfully: {Email}", user.Email);
            return (true, null, user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during authentication for email {Email}", email);
            return (false, "An unexpected error occurred during login. Please try again.", null);
        }
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        return await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);
    }

    public async Task UpdateLastLoginAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}
