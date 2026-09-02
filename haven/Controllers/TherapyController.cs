using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Haven.Data;
using Haven.Models;
using Haven.Services;
using System.Security.Claims;

namespace Haven.Controllers;

public class TherapyController : Controller
{
    private readonly HavenDbContext _db;

    public TherapyController(HavenDbContext db)
    {
        _db = db;
    }

    public IActionResult Index(string specialty = "All", string mode = "All")
    {
        var therapists = HavenDataStore.GetTherapists();

        if (!string.IsNullOrEmpty(specialty) && specialty != "All")
        {
            therapists = therapists.Where(t => 
                t.SpecializationsEn.Any(s => s.Contains(specialty, StringComparison.OrdinalIgnoreCase)) ||
                t.SpecializationsBn.Any(s => s.Contains(specialty, StringComparison.OrdinalIgnoreCase))).ToList();
        }

        var allSpecsEn = new List<string> { "All", "Adolescent Trauma", "Clinical Depression", "Cyber Harassment Recovery", "Child Sexual Abuse Recovery", "Panic & OCD", "Substance & Screen Addiction" };
        var allSpecsBn = new List<string> { "সকল", "কিশোর ট্রমা", "ক্লিনিক্যাল ডিপ্রেশন", "সাইবার হয়রানি পরবর্তী চিকিৎসা", "শিশু যৌন নির্যাতন নিরাময়", "প্যানিক ও ওসিডি", "মাদক ও স্ক্রিন আসক্তি মুক্তি" };

        var model = new TherapyDirectoryViewModel
        {
            Therapists = therapists,
            AllSpecializationsEn = allSpecsEn,
            AllSpecializationsBn = allSpecsBn,
            SelectedSpecialty = specialty,
            SelectedMode = mode
        };

        return View(model);
    }

    // FR-5 / UC-16: Book Confidential Therapy Appointment
    [HttpPost]
    public async Task<IActionResult> BookSession([FromBody] BookingRequest request)
    {
        if (request == null || request.TherapistId <= 0)
        {
            return BadRequest(new { success = false, message = "Invalid booking details." });
        }

        int userId = 0;
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(idClaim, out int claimId))
        {
            userId = claimId;
        }

        // Database record insertion for appointment (FR-5, UC-16)
        if (userId > 0)
        {
            var appointment = new Appointment
            {
                UserId = userId,
                ProfessionalId = request.TherapistId,
                ScheduledDate = DateTime.TryParse(request.Date, out DateTime dt) ? dt : DateTime.UtcNow.AddDays(1),
                TimeSlot = string.IsNullOrWhiteSpace(request.Time) ? "04:30 PM" : request.Time,
                Status = "Scheduled",
                CommunicationChannel = request.ContactMethod ?? "Encrypted Session",
                Notes = $"Anonymous: {request.IsAnonymous}, Fee Subsidy: {request.RequestFeeSubsidy}",
                CreatedAt = DateTime.UtcNow
            };

            _db.Appointments.Add(appointment);
            await _db.SaveChangesAsync();
        }

        var bookingCode = "HVN-SLOT-" + Random.Shared.Next(10000, 99999);
        return Json(new
        {
            success = true,
            bookingCode,
            therapistId = request.TherapistId,
            date = request.Date,
            time = request.Time,
            isAnonymous = request.IsAnonymous,
            messageEn = $"Your confidential session is booked! Reference code: {bookingCode}. Encrypted session link sent.",
            messageBn = $"আপনার গোপনীয় সেশনটি নিশ্চিত হয়েছে! রেফারেন্স কোড: {bookingCode}। এনক্রিপ্ট করা সেশন লিংক পাঠানো হয়েছে।"
        });
    }

    // FR-5 / Reschedule or Cancel Appointment
    [HttpPost]
    public async Task<IActionResult> CancelAppointment(int appointmentId)
    {
        var appt = await _db.Appointments.FindAsync(appointmentId);
        if (appt != null)
        {
            appt.Status = "Cancelled";
            await _db.SaveChangesAsync();
            return Json(new { success = true, message = "Appointment cancelled / বুকিং বাতিল করা হয়েছে।" });
        }
        return Json(new { success = false, message = "Appointment not found." });
    }

    // FR-10 / UC-24: Therapist / Professional Credential Application Portal
    [HttpGet]
    public async Task<IActionResult> Apply()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            TempData["InfoMessage"] = "থেরাপিস্ট বা পেশাদার হিসেবে আবেদন করতে অনুগ্রহ করে আপনার অ্যাকাউন্টে লগইন করুন। / Please log in to submit your professional verification application.";
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Apply", "Therapy") });
        }

        int userId = GetUserId();
        var existingProfile = await _db.ProfessionalProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        ViewBag.ExistingProfile = existingProfile;

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Apply(string titleEn, string titleBn, string specialty, string licenseNo, decimal hourlyRateBDT)
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return RedirectToAction("Login", "Account");
        }

        if (string.IsNullOrWhiteSpace(titleEn) || string.IsNullOrWhiteSpace(licenseNo) || string.IsNullOrWhiteSpace(specialty))
        {
            TempData["ErrorMessage"] = "অনুগ্রহ করে আপনার ডিগ্রি/পদবি, স্পেশালিটি ও বিএমডিসি লাইসেন্স নম্বর প্রদান করুন। / Title, Specialty, and BMDC License No are required.";
            return RedirectToAction(nameof(Apply));
        }

        int userId = GetUserId();
        var profile = await _db.ProfessionalProfiles.FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile == null)
        {
            profile = new ProfessionalProfile
            {
                UserId = userId,
                TitleEn = titleEn.Trim(),
                TitleBn = string.IsNullOrWhiteSpace(titleBn) ? titleEn.Trim() : titleBn.Trim(),
                Specialty = specialty.Trim(),
                LicenseNo = licenseNo.Trim(),
                HourlyRateBDT = hourlyRateBDT > 0 ? hourlyRateBDT : 500,
                ApprovalStatus = "Pending",
                IsBmdcVerified = false,
                SubmittedAt = DateTime.UtcNow
            };
            _db.ProfessionalProfiles.Add(profile);
        }
        else
        {
            profile.TitleEn = titleEn.Trim();
            profile.TitleBn = string.IsNullOrWhiteSpace(titleBn) ? titleEn.Trim() : titleBn.Trim();
            profile.Specialty = specialty.Trim();
            profile.LicenseNo = licenseNo.Trim();
            profile.HourlyRateBDT = hourlyRateBDT > 0 ? hourlyRateBDT : 500;
            profile.ApprovalStatus = "Pending";
            profile.SubmittedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = "আপনার থেরাপিস্ট ভেরিফিকেশন আবেদনটি সফলভাবে অ্যাডমিন প্যানেলে জমা দেওয়া হয়েছে! যাচাইকরণ শেষে আপনার অ্যাকাউন্ট অনুমোদন করা হবে। / Your verification application has been submitted for admin approval!";
        return RedirectToAction(nameof(Apply));
    }

    private int GetUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(idClaim, out int uid) ? uid : 0;
    }
}

public class BookingRequest
{
    public int TherapistId { get; set; }
    public int SlotId { get; set; }
    public string Date { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public string Name { get; set; } = "Anonymous Patient";
    public string ContactMethod { get; set; } = "Signal / WhatsApp";
    public string ContactValue { get; set; } = string.Empty;
    public bool IsAnonymous { get; set; } = true;
    public string PaymentMode { get; set; } = "bkash";
    public bool RequestFeeSubsidy { get; set; } = false;
}
