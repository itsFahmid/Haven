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
