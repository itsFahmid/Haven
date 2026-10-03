using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Obhoy.Services;

namespace Obhoy.Controllers;

[Authorize]
public class NotificationController : Controller
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationController> _logger;

    public NotificationController(INotificationService notificationService, ILogger<NotificationController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    // Get current user's notifications JSON (for Bell Dropdown and live polling)
    [HttpGet]
    public async Task<IActionResult> GetNotifications()
    {
        int userId = GetCurrentUserId();
        if (userId <= 0)
        {
            return Unauthorized();
        }

        var notifications = await _notificationService.GetUserNotificationsAsync(userId, take: 15);
        var unreadCount = await _notificationService.GetUnreadCountAsync(userId);

        var data = notifications.Select(n => new
        {
            id = n.Id,
            title = n.Title,
            message = n.Message,
            type = n.Type,
            linkUrl = n.LinkUrl ?? "#",
            isRead = n.IsRead,
            createdAt = n.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            timeAgo = GetTimeAgo(n.CreatedAt)
        });

        return Json(new
        {
            success = true,
            unreadCount = unreadCount,
            notifications = data
        });
    }

    // Mark single notification as read
    [HttpPost]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        int userId = GetCurrentUserId();
        if (userId <= 0) return Unauthorized();

        bool success = await _notificationService.MarkAsReadAsync(id, userId);
        int unreadCount = await _notificationService.GetUnreadCountAsync(userId);

        return Json(new { success, unreadCount });
    }

    // Mark all notifications as read
    [HttpPost]
    public async Task<IActionResult> MarkAllAsRead()
    {
        int userId = GetCurrentUserId();
        if (userId <= 0) return Unauthorized();

        bool success = await _notificationService.MarkAllAsReadAsync(userId);
        return Json(new { success, unreadCount = 0 });
    }

    private int GetCurrentUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(idClaim, out int uid) ? uid : 0;
    }

    private static string GetTimeAgo(DateTime dateTime)
    {
        var span = DateTime.UtcNow - dateTime;
        if (span.TotalMinutes < 1) return "Just now / এইমাত্র";
        if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes}m ago / {(int)span.TotalMinutes} মিনিট আগে";
        if (span.TotalHours < 24) return $"{(int)span.TotalHours}h ago / {(int)span.TotalHours} ঘণ্টা আগে";
        if (span.TotalDays < 7) return $"{(int)span.TotalDays}d ago / {(int)span.TotalDays} দিন আগে";
        return dateTime.ToString("dd MMM yyyy");
    }
}
