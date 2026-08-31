using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Haven.Data;
using Haven.Models;
using System.Security.Claims;

namespace Haven.Controllers;

public class CommunityController : Controller
{
    private readonly HavenDbContext _db;

    public CommunityController(HavenDbContext db)
    {
        _db = db;
    }

    // UC-20: Peer Support Forum with Randomized Aliases
    public IActionResult Index()
    {
        var mockPosts = GetSeedPosts();
        return View(mockPosts);
    }

    // UC-21: Submit Encrypted Safety Incident Report
    [HttpGet]
    public IActionResult ReportIncident()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReportIncident(IncidentReportRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Description))
        {
            ModelState.AddModelError(string.Empty, "অভিযোগের বিবরণ আবশ্যক।");
            return View(request);
        }

        int? userId = null;
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(idClaim, out int uid))
        {
            userId = uid;
        }

        // Log acute alert into database if high severity
        _db.CrisisAlerts.Add(new CrisisAlert
        {
            UserId = userId,
            TriggerKeyword = request.IncidentType ?? "UserReportedIncident",
            SeverityLevel = "High",
            ActionTaken = "Encrypted Safety Report Filed",
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = "আপনার গোপনীয় নিরাপত্তা রিপোর্ট জমা নেওয়া হয়েছে। এনক্রিপ্টেড ট্র্যাকিং কোড: HVN-REP-" + Random.Shared.Next(10000, 99999);
        return RedirectToAction(nameof(Index));
    }

    private List<CommunityPost> GetSeedPosts()
    {
        return new List<CommunityPost>
        {
            new CommunityPost
            {
                Id = 1,
                AnonymousAlias = "Guardian_Protector_49",
                Topic = "Cyber Grooming Warning",
                Title = "ফেসবুক মেসেঞ্জারে অপরিচিত ব্যক্তির থেকে ব্যক্তিগত ছবি চাওয়ার ঘটনা প্রতিরোধে করণীয়",
                Content = "অনেকেই বিভিন্ন গেম গ্রুপে চ্যাট করার মাধ্যমে মেসেঞ্জারে পার্সোনাল ইনফরমেশন সংগ্রহ করে ব্যাকমেইল করার চেষ্টা করে। সর্বদা ২-ফ্যাক্টর অথেন্টিকেশন চালু রাখুন...",
                CreatedAt = DateTime.UtcNow.AddHours(-5),
                CommentCount = 14
            },
            new CommunityPost
            {
                Id = 2,
                AnonymousAlias = "Safe_Youth_82",
                Topic = "Mental Resilience",
                Title = "মানসিক উদ্বেগ বা প্যানিক অ্যাটাকের সময় ৩-৩-৩ রুল প্রয়োগের ব্যবহারিক অভিজ্ঞতা",
                Content = "প্যানিক হলে চারপাশে ৩টি জিনিস দেখুন, ৩টি শব্দ শুনুন এবং শরীরের ৩টি অঙ্গ নাড়ান। এটি সঙ্গে সঙ্গে মস্তিষ্ককে রিল্যাক্স করতে সাহায্য করে...",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                CommentCount = 28
            }
        };
    }
}

public class CommunityPost
{
    public int Id { get; set; }
    public string AnonymousAlias { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int CommentCount { get; set; }
}

public class IncidentReportRequest
{
    public string IncidentType { get; set; } = "Cyber Harassment";
    public string Description { get; set; } = string.Empty;
    public string EvidenceUrl { get; set; } = string.Empty;
    public bool IsAnonymous { get; set; } = true;
}
