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
        var query = _db.Articles.AsQueryable();

        if (!string.IsNullOrEmpty(category) && category != "All")
        {
            query = query.Where(a => a.Category == category);
        }

        var articles = await query.OrderByDescending(a => a.CreatedAt).ToListAsync();

        // Fallback seed articles if database is currently empty
        if (!articles.Any())
        {
            articles = GetSeedArticles();
        }

        ViewData["SelectedCategory"] = category;
        return View(articles);
    }

    public async Task<IActionResult> Details(int id)
    {
        var article = await _db.Articles.Include(a => a.Author).FirstOrDefaultAsync(a => a.Id == id);
        
        if (article == null)
        {
            article = GetSeedArticles().FirstOrDefault(a => a.Id == id);
        }

        if (article == null)
        {
            return NotFound();
        }

        return View(article);
    }

    // Clinician Article Publishing Form (FR-9)
    [Authorize]
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Article article)
    {
        if (string.IsNullOrWhiteSpace(article.TitleBn) || string.IsNullOrWhiteSpace(article.ContentMarkdown))
        {
            ModelState.AddModelError(string.Empty, "নিবন্ধের শিরোনাম ও মূল বিষয়বস্তু আবশ্যক।");
            return View(article);
        }

        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(idClaim, out int userId))
        {
            article.AuthorId = userId;
        }

        article.CreatedAt = DateTime.UtcNow;
        article.ApprovalStatus = "Approved";

        _db.Articles.Add(article);
        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = "আপনার নিবন্ধটি সফলভাবে প্রকাশিত হয়েছে!";
        return RedirectToAction(nameof(Index));
    }

    private List<Article> GetSeedArticles()
    {
        return new List<Article>
        {
            new Article
            {
                Id = 1,
                TitleBn = "অনলাইন গ্রুমিং চেনার ১০টি পূর্বলক্ষণ ও প্রতিরোধ নির্দেশিকা",
                TitleEn = "10 Early Warning Signs of Online Grooming and Prevention Guide",
                Category = "Grooming Prevention",
                ContentMarkdown = "অনলাইন প্ল্যাটফর্মে কিশোর ও শিশুদের সাথে বন্ধুত্ব তৈরি করে গোপনীয় তথ্য হাসিল করা এবং পরবর্তীতে ব্ল্যাকমেইল করার প্রক্রিয়াকে গ্রুমিং বলা হয়। এই নিবন্ধে গ্রুমিং চেনার কৌশল এবং আইনি পদক্ষেপ সম্পর্কে বিস্তারিত আলোচনা করা হলো...",
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new Article
            {
                Id = 2,
                TitleBn = "সাইবার বুলিং ও ডিজিটাল ব্ল্যাকমেইল মোকাবিলায় আইনি ও প্রযুক্তিগত পদক্ষেপ",
                TitleEn = "Legal and Technical Measures Against Cyber Blackmail in Bangladesh",
                Category = "Cyber Harassment",
                ContentMarkdown = "বাংলাদেশে ডিজিটাল নিরাপত্তা আইন ও সাইবার সুরক্ষা ফ্রেমওয়ার্কের অধীনে যেকোনো ব্ল্যাকমেইল বা ভুয়া আইডি খোলার বিরুদ্ধে দ্রুততম সময়ে সিআইডি সাইবার পুলিশ সেন্টারে অভিযোগ জমা দেওয়ার সহজ পদ্ধতি...",
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            }
        };
    }
}
