using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Haven.Data;
using Haven.Models;
using System.Security.Claims;

namespace Haven.Controllers;

public class CommunityController : Controller
{
    private readonly HavenDbContext _db;
    private readonly ILogger<CommunityController> _logger;

    public CommunityController(HavenDbContext db, ILogger<CommunityController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // 18+ Age Gate Protection Check
    private async Task<(bool Allowed, string Reason, User? User)> Check18PlusAgeGateAsync()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return (false, "কমিউনিটি প্ল্যাটফর্মে যুক্ত হতে ও অন্যদের গল্প ও অভিজ্ঞতা পড়তে অনুগ্রহ করে প্রথমে আপনার অ্যাকাউন্টে লগইন করুন। / Please log in to access the 18+ peer support community.", null);
        }

        int userId = GetUserId();
        var user = await _db.Users.FindAsync(userId);
        if (user == null)
        {
            return (false, "ইউজার অ্যাকাউন্ট পাওয়া যায়নি।", null);
        }

        // Age Gate Verification (Must be 18+)
        if (user.Age.HasValue && user.Age.Value < 18)
        {
            return (false, "সুরক্ষা নীতি অনুযায়ী HAVEN কমিউনিটি সেকশনটি শুধুমাত্র ১৮+ বয়সের তরুণদের জন্য সংরক্ষিত। আপনার বয়স ১৮ এর কম হওয়ায় সরাসরি প্রবেশাধিকার সুরক্ষিত। / Under HAVEN safety policy, the peer support community is restricted to 18+ users.", user);
        }

        return (true, string.Empty, user);
    }

    // 18+ Peer Support Forum
    public async Task<IActionResult> Index(string category = "All")
    {
        await SeedInitialPostsIfEmptyAsync();

        var (allowed, reason, currentUser) = await Check18PlusAgeGateAsync();
        if (!allowed)
        {
            ViewBag.AgeGateReason = reason;
            ViewBag.IsUnderage = currentUser?.Age < 18;
            return View("AgeGateLocked");
        }

        var query = _db.CommunityPosts
            .Include(p => p.User)
            .Include(p => p.Comments)
                .ThenInclude(c => c.User)
            .AsQueryable();

        if (!string.IsNullOrEmpty(category) && category != "All")
        {
            query = query.Where(p => p.Category == category);
        }

        var posts = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();

        ViewData["SelectedCategory"] = category;
        return View(posts);
    }

    // "Post Here" - Share Story / Experience Form
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> CreatePost()
    {
        var (allowed, reason, _) = await Check18PlusAgeGateAsync();
        if (!allowed)
        {
            TempData["ErrorMessage"] = reason;
            return RedirectToAction(nameof(Index));
        }

        return View(new CommunityPost());
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePost(CommunityPost post)
    {
        var (allowed, reason, _) = await Check18PlusAgeGateAsync();
        if (!allowed)
        {
            TempData["ErrorMessage"] = reason;
            return RedirectToAction(nameof(Index));
        }

        if (string.IsNullOrWhiteSpace(post.Title) || string.IsNullOrWhiteSpace(post.Content))
        {
            ModelState.AddModelError(string.Empty, "পোস্টের শিরোনাম ও গল্প/অভিজ্ঞতার বর্ণনা আবশ্যক। / Post title and story content are required.");
            return View(post);
        }

        int userId = GetUserId();
        post.UserId = userId;
        post.CreatedAt = DateTime.UtcNow;
        post.LikeCount = 0;
        post.ReportCount = 0;
        post.IsReported = false;

        _db.CommunityPosts.Add(post);
        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = "আপনার গল্প/অভিজ্ঞতা সফলভাবে শেয়ার করা হয়েছে! / Your story has been posted successfully!";
        return RedirectToAction(nameof(Index));
    }

    // Add Comment on Post
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment(int postId, string commentText, bool isAnonymous = true)
    {
        var (allowed, reason, _) = await Check18PlusAgeGateAsync();
        if (!allowed)
        {
            TempData["ErrorMessage"] = reason;
            return RedirectToAction(nameof(Index));
        }

        if (string.IsNullOrWhiteSpace(commentText))
        {
            TempData["ErrorMessage"] = "মন্তব্যের বিবরণ প্রদান করুন। / Comment text cannot be empty.";
            return RedirectToAction(nameof(Index));
        }

        var post = await _db.CommunityPosts.FindAsync(postId);
        if (post == null) return NotFound();

        int userId = GetUserId();
        var comment = new CommunityComment
        {
            PostId = postId,
            UserId = userId,
            CommentText = commentText.Trim(),
            IsAnonymous = isAnonymous,
            CreatedAt = DateTime.UtcNow
        };

        _db.CommunityComments.Add(comment);
        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = "আপনার মন্তব্য যুক্ত হয়েছে! / Comment added successfully!";
        return RedirectToAction(nameof(Index));
    }

    // Report Post
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReportPost(int postId, string reason)
    {
        var post = await _db.CommunityPosts.FindAsync(postId);
        if (post == null) return NotFound();

        int userId = GetUserId();

        bool alreadyReported = await _db.PostReports.AnyAsync(r => r.PostId == postId && r.ReporterUserId == userId);
        if (alreadyReported)
        {
            TempData["InfoMessage"] = "আপনি ইতিমধ্যে এই পোস্টটির বিরুদ্ধে রিপোর্ট করেছেন। / You have already reported this post.";
            return RedirectToAction(nameof(Index));
        }

        var report = new PostReport
        {
            PostId = postId,
            ReporterUserId = userId,
            Reason = string.IsNullOrWhiteSpace(reason) ? "Inappropriate Content" : reason.Trim(),
            ReportedAt = DateTime.UtcNow
        };

        _db.PostReports.Add(report);
        post.ReportCount += 1;
        post.IsReported = true;

        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = "পোস্টটির বিরুদ্ধে রিপোর্ট জমা নেওয়া হয়েছে। অ্যাডমিন প্যানেল এটি পর্যালোচনা করবে। / Report submitted for admin review.";
        return RedirectToAction(nameof(Index));
    }

    // Legacy Incident Report action maintained for backwards compatibility
    [HttpGet]
    public IActionResult ReportIncident()
    {
        return RedirectToAction(nameof(CreatePost));
    }

    private int GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out int id) ? id : 0;
    }

    private async Task SeedInitialPostsIfEmptyAsync()
    {
        if (!await _db.CommunityPosts.AnyAsync())
        {
            var p1 = new CommunityPost
            {
                Title = "ফেসবুক মেসেঞ্জারে অপরিচিত ব্যক্তির থেকে ব্ল্যাকমেইল চেষ্টার পর যেভাবে নিজেকে নিরাপদ রেখেছিলাম",
                Content = "অনেকেই বিভিন্ন গেম বা ট্রাভেল গ্রুপে চ্যাট করার মাধ্যমে মেসেঞ্জারে পার্সোনাল ইনফরমেশন সংগ্রহ করে ব্ল্যাকমেইল করার চেষ্টা করে। প্রথমত কোনো মন্তব্য না করে স্ক্রিনশট নেওয়া এবং ২-ফ্যাক্টর সিকিউরিটি চালু করা সবচেয়ে জরুরি...",
                Category = "Experience",
                IsAnonymous = true,
                CreatedAt = DateTime.UtcNow.AddHours(-5),
                LikeCount = 12
            };

            var p2 = new CommunityPost
            {
                Title = "মানসিক উদ্বেগ বা প্যানিক অ্যাটাকের সময় ৩-৩-৩ রুল প্রয়োগের আমার বাস্তব অভিজ্ঞতা",
                Content = "প্যানিক বা তীব্র ভীতি হলে চারপাশে ৩টি জিনিস দেখুন, ৩টি শব্দ শুনুন এবং শরীরের ৩টি অঙ্গ নাড়ান। এটি সঙ্গে সঙ্গে মস্তিষ্ককে রিল্যাক্স করতে সাহায্য করে...",
                Category = "Story",
                IsAnonymous = false,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                LikeCount = 28
            };

            _db.CommunityPosts.AddRange(p1, p2);
            await _db.SaveChangesAsync();

            _db.CommunityComments.Add(new CommunityComment
            {
                PostId = p1.Id,
                CommentText = "খুবই প্রয়োজনীয় অভিজ্ঞতা শেয়ার করার জন্য ধন্যবাদ! ২-ফ্যাক্টর প্রমাণীকরণ সত্যিই জীবন বাঁচায়।",
                IsAnonymous = true,
                CreatedAt = DateTime.UtcNow.AddHours(-3)
            });

            _db.CommunityComments.Add(new CommunityComment
            {
                PostId = p2.Id,
                CommentText = "আমি গত সপ্তাহে পরীক্ষায় প্যানিক হওয়ার সময় এটি ট্রাই করেছিলাম, দারুণ কাজ করেছিল!",
                IsAnonymous = false,
                CreatedAt = DateTime.UtcNow.AddHours(-12)
            });

            await _db.SaveChangesAsync();
        }
    }
}

public class IncidentReportRequest
{
    public string? IncidentType { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsAnonymous { get; set; } = true;
}
