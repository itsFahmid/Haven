using Microsoft.EntityFrameworkCore;
using Obhoy.Data;
using Obhoy.Models;

namespace Obhoy.Services;

public class NotificationService : INotificationService
{
    private readonly ObhoyDbContext _db;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(ObhoyDbContext db, ILogger<NotificationService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<Notification> CreateNotificationAsync(int userId, string title, string message, string type = "General", string? linkUrl = null)
    {
        var notification = new Notification
        {
            UserId = userId,
            Title = title.Trim(),
            Message = message.Trim(),
            Type = type,
            LinkUrl = linkUrl,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Created notification #{NotificationId} for User {UserId} ({Type})",
            notification.Id, userId, type);

        return notification;
    }

    public async Task<List<Notification>> GetUserNotificationsAsync(int userId, int take = 20)
    {
        return await _db.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(take)
            .ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync(int userId)
    {
        return await _db.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    public async Task<bool> MarkAsReadAsync(int notificationId, int userId)
    {
        var notification = await _db.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (notification == null) return false;

        notification.IsRead = true;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> MarkAllAsReadAsync(int userId)
    {
        var unread = await _db.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        if (!unread.Any()) return true;

        foreach (var item in unread)
        {
            item.IsRead = true;
        }

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task NotifyAdminsAsync(string title, string message, string type = "AdminAlert", string? linkUrl = null)
    {
        try
        {
            var adminUsers = await _db.Users
                .Where(u => u.Role == "Admin" && u.IsActive)
                .ToListAsync();

            foreach (var admin in adminUsers)
            {
                _db.Notifications.Add(new Notification
                {
                    UserId = admin.Id,
                    Title = title.Trim(),
                    Message = message.Trim(),
                    Type = type,
                    LinkUrl = linkUrl,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to broadcast notification to Admins");
        }
    }
}
