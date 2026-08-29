using Microsoft.AspNetCore.Mvc;
using Haven.Models;
using Haven.Services;

namespace Haven.Controllers;

public class TherapyController : Controller
{
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

    [HttpPost]
    public IActionResult BookSession([FromBody] BookingRequest request)
    {
        if (request == null || request.TherapistId <= 0)
        {
            return BadRequest(new { success = false, message = "Invalid booking details." });
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
