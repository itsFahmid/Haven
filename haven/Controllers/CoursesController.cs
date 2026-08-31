using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Haven.Data;
using Haven.Models;
using Haven.Services;
using System.Security.Claims;

namespace Haven.Controllers;

public class CoursesController : Controller
{
    private readonly HavenDbContext _db;

    public CoursesController(HavenDbContext db)
    {
        _db = db;
    }

    // UC-08: Browse Courses (Publicly accessible to Guest & Registered Users)
    public async Task<IActionResult> Index(string category = "All", bool enrolledOnly = false)
    {
        var allCourses = HavenDataStore.GetCourses();
        var filteredCourses = allCourses;

        if (enrolledOnly && User.Identity?.IsAuthenticated == true)
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(idClaim, out int userId))
            {
                var enrolledIds = await _db.Enrollments
                    .Where(e => e.UserId == userId)
                    .Select(e => e.CourseId)
                    .ToListAsync();

                // If user just registered and hasn't enrolled in DB yet, show default initial enrolled courses (e.g. course 1 and 2)
                if (!enrolledIds.Any())
                {
                    enrolledIds = new List<int> { 1, 2 };
                }

                filteredCourses = allCourses.Where(c => enrolledIds.Contains(c.Id)).ToList();
            }
        }
        else if (!string.IsNullOrEmpty(category) && category != "All")
        {
            filteredCourses = allCourses.Where(c => 
                c.CategoryEn.Equals(category, StringComparison.OrdinalIgnoreCase) || 
                c.CategoryBn.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        var model = new CoursesHubViewModel
        {
            SelectedCategory = category,
            Courses = filteredCourses,
            CategoriesEn = new() { "All", "Cyber Safety", "Personal Safety", "Mental Health", "Parenting & Guardians" },
            CategoriesBn = new() { "সকল", "সাইবার নিরাপত্তা", "ব্যক্তিগত সুরক্ষা", "মানসিক স্বাস্থ্য", "অভিভাবকত্ব ও গাইডেন্স" },
            TotalLearnersCount = 18450,
            CertificatesIssued = 9230
        };

        ViewData["IsEnrolledOnly"] = enrolledOnly;
        return View("Index", model);
    }

    // Direct My Courses endpoint for dropdown navigation
    public async Task<IActionResult> MyCourses()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("MyCourses", "Courses") });
        }

        return await Index(category: "All", enrolledOnly: true);
    }

    // UC-10 / UC-11: Access Course Details & Lessons (Requires Account / Login per UC-11 spec)
    public async Task<IActionResult> Details(int id)
    {
        // Require authentication to access course lessons and interactive content
        if (User.Identity?.IsAuthenticated != true)
        {
            TempData["InfoMessage"] = "কোর্সের পাঠে প্রবেশ করতে ও আপনার অগ্রগতি সংরক্ষণ করতে অনুগ্রহ করে প্রথমে একটি ফ্রি অ্যাকাউন্টে লগইন করুন।";
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Details", "Courses", new { id }) });
        }

        var course = HavenDataStore.GetCourses().FirstOrDefault(c => c.Id == id);
        if (course == null)
        {
            return NotFound();
        }

        // Record or update user enrollment in DB
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(idClaim, out int userId))
        {
            var enrollment = await _db.Enrollments.FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == id);
            if (enrollment == null)
            {
                _db.Enrollments.Add(new Enrollment
                {
                    UserId = userId,
                    CourseId = id,
                    ProgressPercentage = 25,
                    EnrolledAt = DateTime.UtcNow,
                    IsCompleted = false
                });
                await _db.SaveChangesAsync();
            }
        }

        return View(course);
    }

    // UC-12: Module Progress Checkbox (Requires Authenticated Session)
    [HttpPost]
    public async Task<IActionResult> ToggleModuleProgress(int courseId, int stepNumber, bool isCompleted)
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return Json(new { 
                success = false, 
                message = "Session expired. Please log in to record progress." 
            });
        }

        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(idClaim, out int userId))
        {
            var enrollment = await _db.Enrollments.FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);
            if (enrollment != null)
            {
                enrollment.ProgressPercentage = isCompleted ? Math.Min(100, enrollment.ProgressPercentage + 25) : Math.Max(0, enrollment.ProgressPercentage - 25);
                enrollment.IsCompleted = enrollment.ProgressPercentage >= 100;
                await _db.SaveChangesAsync();
            }
        }

        return Json(new { 
            success = true, 
            courseId, 
            stepNumber, 
            isCompleted,
            message = isCompleted ? "মডিউল সম্পন্ন হিসেবে চিহ্নিত হয়েছে!" : "মডিউল আবার সক্রিয় করা হয়েছে।" 
        });
    }
}
