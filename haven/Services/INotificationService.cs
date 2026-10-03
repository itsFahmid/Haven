using Obhoy.Models;

namespace Obhoy.Services;

public interface INotificationService
{
    Task<Notification> CreateNotificationAsync(int userId, string title, string message, string type = "General", string? linkUrl = null);
    Task<List<Notification>> GetUserNotificationsAsync(int userId, int take = 20);
    Task<int> GetUnreadCountAsync(int userId);
    Task<bool> MarkAsReadAsync(int notificationId, int userId);
    Task<bool> MarkAllAsReadAsync(int userId);
    Task NotifyAdminsAsync(string title, string message, string type = "AdminAlert", string? linkUrl = null);
}
