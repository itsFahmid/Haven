using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Haven.Data;
using Haven.Models;

namespace Haven.Controllers;

[Authorize]
public class BookingController : Controller
{
    private readonly HavenDbContext _db;
    private readonly ILogger<BookingController> _logger;

    public BookingController(HavenDbContext db, ILogger<BookingController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // 2. Booking Request Action (POST):
    // Submitting the form creates a new Booking entry in the database with status Pending.
    // Contains reference to UserId (Patient) and TherapistId (Professional).
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookingCreateModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "অনুগ্রহ করে সঠিক তারিখ ও সময় নির্বাচন করুন। / Please select a valid date and time slot.";
            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }
            return RedirectToAction("Directory", "Therapist");
        }

        int userId = GetCurrentUserId();
        if (userId <= 0)
        {
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Directory", "Therapist") });
        }

        // Validate that therapist exists and is verified
        var therapist = await _db.ProfessionalProfiles
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == model.TherapistId && (p.ApprovalStatus == "Approved" || p.IsBmdcVerified));

        if (therapist == null)
        {
            TempData["ErrorMessage"] = "The selected therapist is unavailable or not verified.";
            return RedirectToAction("Directory", "Therapist");
        }

        // Prevent booking with oneself
        if (therapist.UserId == userId)
        {
            TempData["ErrorMessage"] = "You cannot book an appointment with yourself.";
            return RedirectToAction("Profile", "Therapist", new { id = model.TherapistId });
        }

        // Ensure date is not in the past
        if (model.BookingDate.Date < DateTime.UtcNow.Date)
        {
            TempData["ErrorMessage"] = "বুকিংয়ের তারিখ অতীতের হতে পারবে না। / Booking date cannot be in the past.";
            return RedirectToAction("Profile", "Therapist", new { id = model.TherapistId });
        }

        var bookingCode = $"HVN-BK-{Random.Shared.Next(10000, 99999)}";

        var booking = new Booking
        {
            UserId = userId,
            TherapistId = model.TherapistId,
            BookingDate = model.BookingDate.Date,
            TimeSlot = model.TimeSlot.Trim(),
            Notes = model.Notes?.Trim(),
            CommunicationMode = string.IsNullOrWhiteSpace(model.CommunicationMode) ? "Online Video" : model.CommunicationMode.Trim(),
            Status = BookingStatus.Pending,
            BookingReference = bookingCode,
            FeeBDT = model.RequestFeeSubsidy ? 0 : therapist.HourlyRateBDT,
            IsFeeSubsidized = model.RequestFeeSubsidy,
            CreatedAt = DateTime.UtcNow
        };

        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Booking request #{BookingId} ({BookingCode}) created by User {UserId} for Therapist {TherapistId} with Status Pending",
            booking.Id, bookingCode, userId, model.TherapistId);

        TempData["SuccessMessage"] = $"আপনার অ্যাপয়েন্টমেন্ট অনুরোধটি (Ref: {bookingCode}) সফলভাবে পাঠানো হয়েছে! থেরাপিস্ট অনুমোদন করলে কনফার্মেশন পাবেন। / Your booking request has been submitted with status 'Pending'!";

        return RedirectToAction(nameof(MyBookings));
    }

    // Patient View: Display all bookings made by the current user
    [HttpGet]
    public async Task<IActionResult> MyBookings()
    {
        int userId = GetCurrentUserId();

        var bookings = await _db.Bookings
            .Include(b => b.Therapist)
                .ThenInclude(t => t!.User)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        var viewModel = new PatientBookingsViewModel
        {
            Bookings = bookings
        };

        return View(viewModel);
    }

    // Patient Cancel Action
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        int userId = GetCurrentUserId();
        var booking = await _db.Bookings.FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

        if (booking == null)
        {
            TempData["ErrorMessage"] = "Booking request not found.";
            return RedirectToAction(nameof(MyBookings));
        }

        if (booking.Status == BookingStatus.Completed)
        {
            TempData["ErrorMessage"] = "Cannot cancel a completed session.";
            return RedirectToAction(nameof(MyBookings));
        }

        booking.Status = BookingStatus.Cancelled;
        booking.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        TempData["InfoMessage"] = $"বুকিং অনুরোধ (Ref: {booking.BookingReference}) বাতিল করা হয়েছে। / Booking request cancelled.";
        return RedirectToAction(nameof(MyBookings));
    }

    private int GetCurrentUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(idClaim, out int uid) ? uid : 0;
    }
}
