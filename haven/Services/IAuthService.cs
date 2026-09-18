using Obhoy.Models;

namespace Obhoy.Services;

public interface IAuthService
{
    Task<(bool Success, string? ErrorMessage, User? User)> RegisterAsync(RegisterViewModel model);
    Task<(bool Success, string? ErrorMessage, User? User)> AuthenticateAsync(string email, string password);
    Task<User?> GetUserByIdAsync(int id);
    Task<User?> GetUserByEmailAsync(string email);
    Task UpdateLastLoginAsync(int userId);
}
