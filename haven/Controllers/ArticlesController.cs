using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Haven.Data;
using Haven.Models;
using System.Security.Claims;

namespace Haven.Controllers;

public class ArticlesController : Controller
{
    private readonly HavenDbContext _db;

    public ArticlesController(HavenDbContext db)
    {
        _db = db;
    }

    // FR-9 / Article Reader Hub
    public async Task<IActionResult> Index(string category = "All")
    {
        await SeedInitialArticlesIfEmptyAsync();

        var query = _db.Articles.Include(a => a.Author).AsQueryable();

        if (!string.IsNullOrEmpty(category) && category != "All")
        {
            query = query.Where(a => a.Category == category);
        }

        var articles = await query.OrderByDescending(a => a.CreatedAt).ToListAsync();

        List<int> bookmarkedArticleIds = new();
        if (User.Identity?.IsAuthenticated == true && int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId))
        {
            bookmarkedArticleIds = await _db.ArticleBookmarks
                .Where(b => b.UserId == userId)
                .Select(b => b.ArticleId)
                .ToListAsync();
        }

        ViewData["SelectedCategory"] = category;
        ViewBag.BookmarkedArticleIds = bookmarkedArticleIds;
        return View(articles);
    }

    public async Task<IActionResult> Details(int id)
    {
        await SeedInitialArticlesIfEmptyAsync();

        var article = await _db.Articles.Include(a => a.Author).FirstOrDefaultAsync(a => a.Id == id);
        if (article == null)
        {
            return NotFound();
        }

        bool isBookmarked = false;
        if (User.Identity?.IsAuthenticated == true && int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId))
        {
            isBookmarked = await _db.ArticleBookmarks.AnyAsync(b => b.UserId == userId && b.ArticleId == id);
        }

        ViewBag.IsBookmarked = isBookmarked;
        return View(article);
    }

    // Bookmark / Unbookmark Article Toggle Action
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleBookmark(int articleId, string? returnUrl = null)
    {
        int userId = GetUserId();
        var article = await _db.Articles.FindAsync(articleId);
        if (article == null) return NotFound();

        var existingBookmark = await _db.ArticleBookmarks
            .FirstOrDefaultAsync(b => b.UserId == userId && b.ArticleId == articleId);

        bool isBookmarked;
        if (existingBookmark != null)
        {
            _db.ArticleBookmarks.Remove(existingBookmark);
            await _db.SaveChangesAsync();
            isBookmarked = false;
            TempData["InfoMessage"] = "নিবন্ধটি বুকমার্ক তালিকা থেকে সরানো হয়েছে। / Article removed from bookmarks.";
        }
        else
        {
            _db.ArticleBookmarks.Add(new ArticleBookmark
            {
                UserId = userId,
                ArticleId = articleId,
                BookmarkedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
            isBookmarked = true;
            TempData["SuccessMessage"] = "নিবন্ধটি আপনার বুকমার্ক তালিকায় যুক্ত করা হয়েছে! / Article saved to your bookmarks!";
        }

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(nameof(Details), new { id = articleId });
    }

    // Clinician Article Publishing Form (FR-9)
    [Authorize]
    [HttpGet]
    public IActionResult Create()
    {
        if (!User.IsInRole("Professional") && !User.IsInRole("Admin"))
        {
            TempData["ErrorMessage"] = "শুধুমাত্র নিবন্ধিত থেরাপিস্ট ও অ্যাডমিনগণ নিবন্ধ প্রকাশ করতে পারবেন।";
            return RedirectToAction(nameof(Index));
        }

        return View();
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Article article)
    {
        if (!User.IsInRole("Professional") && !User.IsInRole("Admin"))
        {
            return RedirectToAction(nameof(Index));
        }

        if (string.IsNullOrWhiteSpace(article.TitleBn) || string.IsNullOrWhiteSpace(article.ContentMarkdown))
        {
            ModelState.AddModelError(string.Empty, "নিবন্ধের শিরোনাম ও মূল বিষয়বস্তু আবশ্যক।");
            return View(article);
        }

        int userId = GetUserId();
        article.AuthorId = userId;
        article.CreatedAt = DateTime.UtcNow;
        article.ApprovalStatus = "Approved";

        _db.Articles.Add(article);
        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = "আপনার নিবন্ধটি সফলভাবে প্রকাশিত হয়েছে!";
        return RedirectToAction(nameof(Index));
    }

    private int GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out int id) ? id : 0;
    }

    private async Task SeedInitialArticlesIfEmptyAsync()
    {
        if (!await _db.Articles.AnyAsync())
        {
            var seedArticles = new List<Article>
            {
                new Article
                {
                    TitleBn = "অনলাইন গ্রুমিং চেনার ১০টি পূর্বলক্ষণ ও প্রতিরোধ নির্দেশিকা",
                    TitleEn = "10 Early Warning Signs of Online Grooming and Prevention Guide",
                    Category = "Grooming Prevention",
                    ContentMarkdown = "অনলাইন প্ল্যাটফর্মে কিশোর ও শিশুদের সাথে বন্ধুত্ব তৈরি করে গোপনীয় তথ্য হাসিল করা এবং পরবর্তীতে ব্ল্যাকমেইল করার প্রক্রিয়াকে গ্রুমিং বলা হয়।\n\n### গ্রুমিং চেনার ১০টি মূল লক্ষণ:\n1. অতিরিক্ত প্রশংসামূলক কথা বলা এবং উপহার পাঠানো।\n2. অভিভাবক বা বন্ধুদের কাছ থেকে চ্যাট লুকানোর জন্য চাপ দেওয়া।\n3. ক্যামেরা অন রাখা বা ব্যক্তিগত ছবি চাওয়ার অন্যায় অনুরোধ।\n\n### আইনি ও টেকনিক্যাল প্রতিরোধ পদক্ষেপ:\n- অবিলম্বে কোনো মন্তব্য ছাড়াই মেসেজের স্ক্রিনশট এবং লিঙ্ক সংরক্ষণ করুন।\n- সিআইডি সাইবার পুলিশ হটলাইন (০১৭৩০০০১৯৯৯) অথবা হেভেনের হটলাইনে যোগাযোগ করুন।",
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                },
                new Article
                {
                    TitleBn = "সাইবার বুলিং ও ডিজিটাল ব্ল্যাকমেইল মোকাবিলায় আইনি ও প্রযুক্তিগত পদক্ষেপ",
                    TitleEn = "Legal and Technical Measures Against Cyber Blackmail in Bangladesh",
                    Category = "Cyber Harassment",
                    ContentMarkdown = "বাংলাদেশে ডিজিটাল নিরাপত্তা আইন ও সাইবার সুরক্ষা ফ্রেমওয়ার্কের অধীনে যেকোনো ব্ল্যাকমেইল বা ভুয়া আইডি খোলার বিরুদ্ধে দ্রুততম সময়ে সিআইডি সাইবার পুলিশ সেন্টারে অভিযোগ জমা দেওয়ার সহজ পদ্ধতি।\n\n### কীভাবে ডিজিটাল প্রমাণ সংরক্ষণ করবেন:\n- স্ক্রিনশটে তারিখ, সময় ও ইউআরএল (URL) স্পষ্ট রাখুন।\n- অপরাধীর সোশ্যাল মিডিয়া প্রোফাইল ইউআরএল কপি করে রাখুন।",
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                },
                new Article
                {
                    TitleBn = "প্যানিক অ্যাটাক ও তীব্র মানসিক উদ্বেগের সময় ৩-৩-৩ রুল প্রয়োগের ব্যবহারিক নির্দেশিকা",
                    TitleEn = "Practical Guide to 3-3-3 Grounding Technique for Anxiety & Panic",
                    Category = "Mental Health",
                    ContentMarkdown = "প্যানিক অ্যাটাকের সময় তাৎক্ষণিক মস্তিষ্ককে শান্ত করার কার্যকর বৈজ্ঞানিক পদ্ধতি হলো ৩-৩-৩ গ্রাউন্ডিং মেথড।\n\n1. **৩টি দৃশ্যমান বস্তু দেখুন**: আপনার চারপাশের ৩টি বস্তু লক্ষ্য করুন।\n2. **৩টি শব্দ শুনুন**: কান দিয়ে ৩টি ভিন্ন শব্দ মনোযোগ সহকারে শুনুন।\n3. **৩টি অঙ্গ নাড়ান**: আপনার হাত, পা বা কাঁধ আস্তে আস্তে নাড়ান।",
                    CreatedAt = DateTime.UtcNow.AddDays(-7)
                }
            };

            _db.Articles.AddRange(seedArticles);
            await _db.SaveChangesAsync();
        }
    }
}
