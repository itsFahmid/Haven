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

    // 1. Therapist Directory (Public/User side):
    // Only show therapists whose profiles are verified (IsVerified == true or ApprovalStatus == "Approved").
    // Unverified or pending therapists must not appear in the directory.
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
}
