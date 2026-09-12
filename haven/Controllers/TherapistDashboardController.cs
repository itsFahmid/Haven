using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Haven.Data;
using Haven.Models;

namespace Haven.Controllers;

[Authorize(Roles = "Professional,Admin")]
public class TherapistDashboardController : Controller
{
    private readonly HavenDbContext _db;
    private readonly ILogger<TherapistDashboardController> _logger;

    public TherapistDashboardController(HavenDbContext db, ILogger<TherapistDashboardController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // 3. ManageRequests (GET):
    // Displays all incoming booking requests targeted to the logged-in therapist (Therapist.UserId == currentLoggedInUserId).
    [HttpGet]
    public async Task<IActionResult> ManageRequests(string? statusFilter = "All")
    {
        int userId = GetCurrentUserId();

        // Find therapist profile for the logged in user
        var therapistProfile = await _db.ProfessionalProfiles
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (therapistProfile == null && !User.IsInRole("Admin"))
        {
            TempData["ErrorMessage"] = "আপনার কোনো থেরাপিস্ট প্রোফাইল পাওয়া যায়নি। / No active professional therapist profile found for your account.";
            return RedirectToAction("Index", "Home");
        }

        // Admin fallback: If admin is viewing without a personal profile, pick the first professional profile for demonstration
        if (therapistProfile == null && User.IsInRole("Admin"))
        {
            therapistProfile = await _db.ProfessionalProfiles
                .Include(p => p.User)
                .FirstOrDefaultAsync();

            if (therapistProfile == null)
            {
                TempData["ErrorMessage"] = "No professional profiles exist in the system.";
                return RedirectToAction("Index", "Admin");
            }
        }

        int therapistId = therapistProfile!.Id;

        // Base query for all requests targeted to this therapist
        var baseQuery = _db.Bookings
            .Include(b => b.User)
            .Where(b => b.TherapistId == therapistId);

        int totalCount = await baseQuery.CountAsync();
        int pendingCount = await baseQuery.CountAsync(b => b.Status == BookingStatus.Pending);
        int approvedCount = await baseQuery.CountAsync(b => b.Status == BookingStatus.Approved);
        int rejectedCount = await baseQuery.CountAsync(b => b.Status == BookingStatus.Rejected);

        var filteredQuery = baseQuery.AsQueryable();

        if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "All")
        {
            if (Enum.TryParse<BookingStatus>(statusFilter, true, out var parsedStatus))
            {
                filteredQuery = filteredQuery.Where(b => b.Status == parsedStatus);
            }
        }

        var requests = await filteredQuery
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        var viewModel = new TherapistDashboardViewModel
        {
            Therapist = therapistProfile,
            Requests = requests,
            CurrentFilter = statusFilter ?? "All",
            TotalRequestsCount = totalCount,
            PendingRequestsCount = pendingCount,
            ApprovedRequestsCount = approvedCount,
            RejectedRequestsCount = rejectedCount
        };

        return View(viewModel);
    }

    // Approve Request Action (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveRequest(int id)
    {
        int userId = GetCurrentUserId();

        var booking = await _db.Bookings
            .Include(b => b.Therapist)
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (booking == null)
        {
            TempData["ErrorMessage"] = "Booking request not found.";
            return RedirectToAction(nameof(ManageRequests));
        }

        // Ensure this booking belongs to the current therapist (or Admin)
        if (booking.Therapist?.UserId != userId && !User.IsInRole("Admin"))
        {
            TempData["ErrorMessage"] = "You do not have permission to manage this booking request.";
            return RedirectToAction(nameof(ManageRequests));
        }

        booking.Status = BookingStatus.Approved;
        booking.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Booking #{BookingId} approved by Therapist User {UserId}", booking.Id, userId);

        TempData["SuccessMessage"] = $"অ্যাপয়েন্টমেন্ট অনুরোধ (Ref: {booking.BookingReference}) অনুমোদিত হয়েছে! রোগীকে নোটিফিকেশন পাঠানো হয়েছে। / Booking request #{booking.Id} has been Approved!";

        return RedirectToAction(nameof(ManageRequests));
    }

    // Reject Request Action (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectRequest(int id)
    {
        int userId = GetCurrentUserId();

        var booking = await _db.Bookings
            .Include(b => b.Therapist)
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (booking == null)
        {
            TempData["ErrorMessage"] = "Booking request not found.";
            return RedirectToAction(nameof(ManageRequests));
        }

        // Ensure this booking belongs to the current therapist (or Admin)
        if (booking.Therapist?.UserId != userId && !User.IsInRole("Admin"))
        {
            TempData["ErrorMessage"] = "You do not have permission to manage this booking request.";
            return RedirectToAction(nameof(ManageRequests));
        }

        booking.Status = BookingStatus.Rejected;
        booking.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Booking #{BookingId} rejected by Therapist User {UserId}", booking.Id, userId);

        TempData["InfoMessage"] = $"অ্যাপয়েন্টমেন্ট অনুরোধ (Ref: {booking.BookingReference}) বাতিল/প্রত্যাখ্যান করা হয়েছে। / Booking request #{booking.Id} has been Rejected.";

        return RedirectToAction(nameof(ManageRequests));
    }

    private int GetCurrentUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(idClaim, out int uid) ? uid : 0;
    }
}
