using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Haven.Data;
using Haven.Models;

namespace Haven.Controllers;

public class TherapistController : Controller
{
    private readonly HavenDbContext _db;
    private readonly ILogger<TherapistController> _logger;

    public TherapistController(HavenDbContext db, ILogger<TherapistController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // 1. Therapist Directory (Public/User side)
    [HttpGet]
    public async Task<IActionResult> Directory(string? specialty = "All", string? search = null)
    {
        var query = _db.ProfessionalProfiles
            .Include(p => p.User)
            .Where(p => (p.ApprovalStatus == "Approved" || p.IsBmdcVerified) && p.User != null && p.User.IsActive);

        if (!string.IsNullOrWhiteSpace(specialty) && specialty != "All")
        {
            query = query.Where(p => p.Specialty.ToLower().Contains(specialty.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(p => (p.User != null && p.User.FullName.ToLower().Contains(s)) ||
                                     p.TitleEn.ToLower().Contains(s) ||
                                     p.TitleBn.ToLower().Contains(s) ||
                                     p.Specialty.ToLower().Contains(s));
        }

        var verifiedTherapists = await query
            .OrderByDescending(p => p.Rating)
            .ThenByDescending(p => p.ExperienceYears)
            .ToListAsync();

        ViewBag.SelectedSpecialty = specialty ?? "All";
        ViewBag.SearchQuery = search ?? "";
        ViewBag.TotalVerified = await _db.ProfessionalProfiles.CountAsync(p => p.ApprovalStatus == "Approved" || p.IsBmdcVerified);

        return View(verifiedTherapists);
    }

    // Therapist Profile Details
    [HttpGet]
    public async Task<IActionResult> Profile(int id)
    {
        if (id <= 0)
        {
            return NotFound();
        }

        var therapist = await _db.ProfessionalProfiles
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id && (p.ApprovalStatus == "Approved" || p.IsBmdcVerified));

        if (therapist == null)
        {
            TempData["ErrorMessage"] = "Therapist profile not found or is currently pending administrative verification.";
            return RedirectToAction(nameof(Directory));
        }

        return View(therapist);
    }

    // 2. Therapist Dashboard / Manage Booking Requests (FIXED DYNAMIC QUERY)
    [Authorize(Roles = "Professional,Admin")]
    [HttpGet]
    public async Task<IActionResult> Dashboard(string? statusFilter = "All")
    {
        int userId = GetCurrentUserId();

        // Fetch therapist profile for the logged in user
        var therapistProfile = await _db.ProfessionalProfiles
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.UserId == userId);

        // Admin fallback: If logged in as Admin and no profile, load the first profile for preview
        if (therapistProfile == null && User.IsInRole("Admin"))
        {
            therapistProfile = await _db.ProfessionalProfiles
                .Include(p => p.User)
                .FirstOrDefaultAsync();
        }

        if (therapistProfile == null)
        {
            TempData["ErrorMessage"] = "আপনার কোনো থেরাপিস্ট প্রোফাইল পাওয়া যায়নি। / No active professional therapist profile found for your account.";
            return RedirectToAction("Index", "Home");
        }

        int profileId = therapistProfile.Id;
        int linkedUserId = therapistProfile.UserId;

        // Dynamic Query: Match either ProfessionalProfile.Id OR User.Id associated with the therapist
        var baseQuery = _db.Bookings
            .Include(b => b.User)
            .Include(b => b.Therapist)
                .ThenInclude(t => t!.User)
            .Where(b => b.TherapistId == profileId || b.TherapistId == linkedUserId);

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

        return View("~/Views/TherapistDashboard/ManageRequests.cshtml", viewModel);
    }

    // Approve Request Action (POST)
    [Authorize(Roles = "Professional,Admin")]
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
            return RedirectToAction(nameof(Dashboard));
        }

        booking.Status = BookingStatus.Approved;
        booking.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Booking #{BookingId} approved by User {UserId}", booking.Id, userId);

        TempData["SuccessMessage"] = $"অ্যাপয়েন্টমেন্ট অনুরোধ (Ref: {booking.BookingReference}) অনুমোদিত হয়েছে! / Booking request #{booking.Id} has been Approved!";

        return RedirectToAction(nameof(Dashboard));
    }

    // Reject Request Action (POST)
    [Authorize(Roles = "Professional,Admin")]
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
            return RedirectToAction(nameof(Dashboard));
        }

        booking.Status = BookingStatus.Rejected;
        booking.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Booking #{BookingId} rejected by User {UserId}", booking.Id, userId);

        TempData["InfoMessage"] = $"অ্যাপয়েন্টমেন্ট অনুরোধ (Ref: {booking.BookingReference}) বাতিল করা হয়েছে। / Booking request #{booking.Id} has been Rejected.";

        return RedirectToAction(nameof(Dashboard));
    }

    private int GetCurrentUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(idClaim, out int uid) ? uid : 0;
    }
}