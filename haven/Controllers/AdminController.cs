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
            .OrderByDescending(p => p.SubmittedAt)
            .ToListAsync();

        var pendingCourses = await _db.Courses
            .Include(c => c.Author)
            .Include(c => c.Modules)
            .Where(c => c.ApprovalStatus == "Pending")
            .ToListAsync();

        var reportedPosts = await _db.CommunityPosts
            .Include(p => p.User)
            .Where(p => p.IsReported || p.ReportCount > 0)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        var recentAlerts = await _db.CrisisAlerts
            .OrderByDescending(c => c.CreatedAt)
            .Take(10)
            .ToListAsync();

        var auditLogs = await _db.AdminAuditLogs
            .OrderByDescending(a => a.ExecutedAt)
            .Take(10)
            .ToListAsync();

        // Dynamic Therapist Counts directly from EF Core DbContext
        var pendingTherapistsCount = await _db.ProfessionalProfiles.CountAsync(p => p.ApprovalStatus == "Pending");
        var approvedTherapistsCount = await _db.ProfessionalProfiles.CountAsync(p => p.ApprovalStatus == "Approved" || p.IsBmdcVerified);
        var rejectedTherapistsCount = await _db.ProfessionalProfiles.CountAsync(p => p.ApprovalStatus == "Rejected");
        var totalTherapistsCount = await _db.ProfessionalProfiles.CountAsync();

        var totalUsersCount = await _db.Users.CountAsync();
        var totalCoursesCount = await _db.Courses.CountAsync();
        var pendingCoursesCount = await _db.Courses.CountAsync(c => c.ApprovalStatus == "Pending");
        var totalAppointmentsCount = await _db.Appointments.CountAsync();
        var reportedPostsCount = await _db.CommunityPosts.CountAsync(p => p.IsReported || p.ReportCount > 0);
        var crisisAlertsCount = await _db.CrisisAlerts.CountAsync();

        var model = new AdminDashboardViewModel
        {
            PendingTherapists = pendingTherapists,
            PendingCourses = pendingCourses,
            ReportedPosts = reportedPosts,
            RecentCrisisAlerts = recentAlerts,
            AuditLogs = auditLogs,

            PendingTherapistsCount = pendingTherapistsCount,
            ApprovedTherapistsCount = approvedTherapistsCount,
            RejectedTherapistsCount = rejectedTherapistsCount,
            TotalTherapistsCount = totalTherapistsCount,

            TotalUsersCount = totalUsersCount,
            TotalCoursesCount = totalCoursesCount,
            PendingCoursesCount = pendingCoursesCount,
            TotalAppointmentsCount = totalAppointmentsCount,
            ReportedPostsCount = reportedPostsCount,
            CrisisAlertsCount = crisisAlertsCount,

            DatabaseProvider = _db.Database.ProviderName ?? "Unknown"
        };

        return View(model);
    }

    // Dismiss User Post Report
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DismissReport(int id)
    {
        var post = await _db.CommunityPosts.FindAsync(id);
        if (post != null)
        {
            post.IsReported = false;
            post.ReportCount = 0;

            int adminId = GetCurrentUserId();
            _db.AdminAuditLogs.Add(new AdminAuditLog
            {
                AdminUserId = adminId,
                ActionType = "DismissReport",
                TargetResource = $"CommunityPost:{id}",
                ActionDetails = $"Dismissed user reports for post #{id}",
                ExecutedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = $"পোস্ট #{id} এর রিপোর্ট বাতিল/খারিজ করা হয়েছে। / Report dismissed for Post #{id}.";
        }
        return RedirectToAction(nameof(Index));
    }

    // Delete Inappropriate Reported Post
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePost(int id)
    {
        var post = await _db.CommunityPosts.FindAsync(id);
        if (post != null)
        {
            var reports = await _db.PostReports.Where(r => r.PostId == id).ToListAsync();
            _db.PostReports.RemoveRange(reports);

            var comments = await _db.CommunityComments.Where(c => c.PostId == id).ToListAsync();
            _db.CommunityComments.RemoveRange(comments);

            _db.CommunityPosts.Remove(post);

            int adminId = GetCurrentUserId();
            _db.AdminAuditLogs.Add(new AdminAuditLog
            {
                AdminUserId = adminId,
                ActionType = "DeleteReportedPost",
                TargetResource = $"CommunityPost:{id}",
                ActionDetails = $"Deleted reported post #{id}: '{post.Title}'",
                ExecutedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
            TempData["InfoMessage"] = $"রিপোর্টকৃত পোস্ট #{id} স্থায়ীভাবে মুছে ফেলা হয়েছে। / Reported Post #{id} deleted.";
        }
        return RedirectToAction(nameof(Index));
    }

    // FR-10 / UC-24: Dedicated Pending Therapist Applications & Credential Verification Portal
    [HttpGet]
    public async Task<IActionResult> PendingTherapists()
    {
        var pendingTherapists = await _db.ProfessionalProfiles
            .Include(p => p.User)
            .Where(p => p.ApprovalStatus == "Pending")
            .OrderByDescending(p => p.SubmittedAt)
            .ToListAsync();

        ViewBag.PendingCount = await _db.ProfessionalProfiles.CountAsync(p => p.ApprovalStatus == "Pending");
        ViewBag.ApprovedCount = await _db.ProfessionalProfiles.CountAsync(p => p.ApprovalStatus == "Approved" || p.IsBmdcVerified);
        ViewBag.RejectedCount = await _db.ProfessionalProfiles.CountAsync(p => p.ApprovalStatus == "Rejected");
        ViewBag.TotalCount = await _db.ProfessionalProfiles.CountAsync();

        return View(pendingTherapists);
    }

    // FR-10 / UC-24: Approve Therapist Credential Verification
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveTherapist(int id, string? returnUrl = null)
    {
        var prof = await _db.ProfessionalProfiles
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (prof != null)
        {
            prof.ApprovalStatus = "Approved";
            prof.IsBmdcVerified = true;
            prof.VerifiedAt = DateTime.UtcNow;

            if (prof.User != null && prof.User.Role != "Admin")
            {
                prof.User.Role = "Professional";
            }

            int adminId = GetCurrentUserId();
            _db.AdminAuditLogs.Add(new AdminAuditLog
            {
                AdminUserId = adminId,
                ActionType = "ApproveTherapist",
                TargetResource = $"ProfessionalProfile:{id}",
                ActionDetails = $"Approved BMDC license #{prof.LicenseNo} for {prof.User?.FullName ?? prof.TitleEn}",
                ExecutedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();

            _logger.LogInformation("Admin {AdminId} approved therapist application #{ProfileId} ({FullName}) with license {LicenseNo}",
                adminId, prof.Id, prof.User?.FullName, prof.LicenseNo);

            TempData["SuccessMessage"] = $"থেরাপিস্ট '{prof.User?.FullName ?? prof.TitleEn}' এর লাইসেন্স (#{prof.LicenseNo}) সফলভাবে অনুমোদন ও ভেরিফাই করা হয়েছে! / Therapist license approved & verified successfully!";
        }
        else
        {
            TempData["ErrorMessage"] = "থেরাপিস্ট প্রোফাইলটি খুঁজে পাওয়া যায়নি। / Therapist profile not found.";
        }

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(nameof(PendingTherapists));
    }

    // FR-10 / UC-24: Reject Therapist Credential
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectTherapist(int id, string? returnUrl = null)
    {
        var prof = await _db.ProfessionalProfiles
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id);

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
                ActionDetails = $"Rejected verification for Profile #{id} ({prof.User?.FullName ?? prof.TitleEn})",
                ExecutedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();

            _logger.LogInformation("Admin {AdminId} rejected therapist application #{ProfileId} ({FullName})",
                adminId, prof.Id, prof.User?.FullName);

            TempData["InfoMessage"] = $"থেরাপিস্ট '{prof.User?.FullName ?? prof.TitleEn}' এর আবেদনটি বাতিল করা হয়েছে। / Therapist application rejected.";
        }
        else
        {
            TempData["ErrorMessage"] = "থেরাপিস্ট প্রোফাইলটি খুঁজে পাওয়া যায়নি। / Therapist profile not found.";
        }

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(nameof(PendingTherapists));
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
    public List<CommunityPost> ReportedPosts { get; set; } = new();
    public List<CrisisAlert> RecentCrisisAlerts { get; set; } = new();
    public List<AdminAuditLog> AuditLogs { get; set; } = new();

    // Dynamic Therapist Counts
    public int PendingTherapistsCount { get; set; }
    public int ApprovedTherapistsCount { get; set; }
    public int RejectedTherapistsCount { get; set; }
    public int TotalTherapistsCount { get; set; }

    // Dynamic Platform Counts
    public int TotalUsersCount { get; set; }
    public int TotalCoursesCount { get; set; }
    public int PendingCoursesCount { get; set; }
    public int TotalAppointmentsCount { get; set; }
    public int ReportedPostsCount { get; set; }
    public int CrisisAlertsCount { get; set; }

    public string DatabaseProvider { get; set; } = string.Empty;
}
