using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Haven.Data;
using Haven.Models;
using System.Security.Claims;

namespace Haven.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly HavenDbContext _db;
    private readonly ILogger<AdminController> _logger;

    public AdminController(HavenDbContext db, ILogger<AdminController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // FR-10, FR-12, UC-22 - UC-25: Admin Dashboard Overview
    public async Task<IActionResult> Index()
    {
        var pendingTherapists = await _db.ProfessionalProfiles
            .Include(p => p.User)
            .Where(p => p.ApprovalStatus == "Pending")
            .ToListAsync();

        var pendingCourses = await _db.Courses
            .Include(c => c.Author)
            .Include(c => c.Modules)
            .Where(c => c.ApprovalStatus == "Pending")
            .ToListAsync();

        var recentAlerts = await _db.CrisisAlerts
            .OrderByDescending(c => c.CreatedAt)
            .Take(10)
            .ToListAsync();

        var auditLogs = await _db.AdminAuditLogs
            .OrderByDescending(a => a.ExecutedAt)
            .Take(10)
            .ToListAsync();

        var model = new AdminDashboardViewModel
        {
            PendingTherapists = pendingTherapists,
            PendingCourses = pendingCourses,
            RecentCrisisAlerts = recentAlerts,
            AuditLogs = auditLogs,
            TotalUsersCount = await _db.Users.CountAsync(),
            TotalCoursesCount = await _db.Courses.CountAsync(),
            TotalAppointmentsCount = await _db.Appointments.CountAsync()
        };

        return View(model);
    }

    // FR-10 / UC-24: Approve Therapist Credential Verification
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveTherapist(int id)
    {
        var prof = await _db.ProfessionalProfiles.FindAsync(id);
        if (prof != null)
        {
            prof.ApprovalStatus = "Approved";
            prof.IsBmdcVerified = true;
            prof.VerifiedAt = DateTime.UtcNow;

            int adminId = GetCurrentUserId();
            _db.AdminAuditLogs.Add(new AdminAuditLog
            {
                AdminUserId = adminId,
                ActionType = "ApproveTherapist",
                TargetResource = $"ProfessionalProfile:{id}",
                ActionDetails = $"Approved BMDC license #{prof.LicenseNo}",
                ExecutedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Therapist #{id} license approved!";
        }
        return RedirectToAction(nameof(Index));
    }

    // FR-10 / UC-24: Reject Therapist Credential
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectTherapist(int id)
    {
        var prof = await _db.ProfessionalProfiles.FindAsync(id);
        if (prof != null)
        {
            prof.ApprovalStatus = "Rejected";
            prof.IsBmdcVerified = false;

            int adminId = GetCurrentUserId();
            _db.AdminAuditLogs.Add(new AdminAuditLog
            {
                AdminUserId = adminId,
                ActionType = "RejectTherapist",
                TargetResource = $"ProfessionalProfile:{id}",
                ActionDetails = $"Rejected verification for Profile #{id}",
                ExecutedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
            TempData["InfoMessage"] = $"Therapist #{id} application rejected.";
        }
        return RedirectToAction(nameof(Index));
    }

    // UC-22 / FR-9: Approve Therapist Submitted Course
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveCourse(int id)
    {
        var course = await _db.Courses.FindAsync(id);
        if (course != null)
        {
            course.ApprovalStatus = "Approved";

            int adminId = GetCurrentUserId();
            _db.AdminAuditLogs.Add(new AdminAuditLog
            {
                AdminUserId = adminId,
                ActionType = "ApproveCourse",
                TargetResource = $"Course:{id}",
                ActionDetails = $"Approved safety course '{course.TitleEn}'",
                ExecutedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = $"কোর্স '{course.TitleEn}' সফলভাবে অনুমোদন করা হয়েছে! / Course approved successfully!";
        }
        return RedirectToAction(nameof(Index));
    }

    // UC-22 / FR-9: Reject Therapist Submitted Course
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectCourse(int id)
    {
        var course = await _db.Courses.FindAsync(id);
        if (course != null)
        {
            course.ApprovalStatus = "Rejected";

            int adminId = GetCurrentUserId();
            _db.AdminAuditLogs.Add(new AdminAuditLog
            {
                AdminUserId = adminId,
                ActionType = "RejectCourse",
                TargetResource = $"Course:{id}",
                ActionDetails = $"Rejected safety course '{course.TitleEn}'",
                ExecutedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
            TempData["InfoMessage"] = $"কোর্স '{course.TitleEn}' বাতিল করা হয়েছে। / Course rejected.";
        }
        return RedirectToAction(nameof(Index));
    }

    private int GetCurrentUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(idClaim, out int uid) ? uid : 1;
    }
}

public class AdminDashboardViewModel
{
    public List<ProfessionalProfile> PendingTherapists { get; set; } = new();
    public List<Course> PendingCourses { get; set; } = new();
    public List<CrisisAlert> RecentCrisisAlerts { get; set; } = new();
    public List<AdminAuditLog> AuditLogs { get; set; } = new();
    public int TotalUsersCount { get; set; }
    public int TotalCoursesCount { get; set; }
    public int TotalAppointmentsCount { get; set; }
}
