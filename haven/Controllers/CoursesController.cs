using Microsoft.AspNetCore.Authorization;
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
    private readonly ILogger<CoursesController> _logger;

    public CoursesController(HavenDbContext db, ILogger<CoursesController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // UC-08 / FR-4: Public Browse Courses Catalog with Pagination & Age Filtering
    public async Task<IActionResult> Index(
        string category = "All",
        string ageGroup = "All",
        string search = "",
        int page = 1)
    {
        await SeedInitialCoursesIfEmptyAsync();

        var query = _db.Courses
            .Include(c => c.Modules)
            .Where(c => c.ApprovalStatus == "Approved");

        // Filter Category
        if (!string.IsNullOrEmpty(category) && category != "All")
        {
            query = query.Where(c => c.CategoryEn.ToLower() == category.ToLower() || c.CategoryBn.ToLower() == category.ToLower());
        }

        // Filter Age Group (Mandatory Tag)
        if (!string.IsNullOrEmpty(ageGroup) && ageGroup != "All")
        {
            query = query.Where(c => c.TargetGen.Contains(ageGroup));
        }

        // Filter Search Keyword
        if (!string.IsNullOrEmpty(search))
        {
            var s = search.ToLower().Trim();
            query = query.Where(c => c.TitleEn.ToLower().Contains(s) || 
                                     c.TitleBn.ToLower().Contains(s) || 
                                     c.DescriptionEn.ToLower().Contains(s) || 
                                     c.DescriptionBn.ToLower().Contains(s));
        }

        int totalCourses = await query.CountAsync();
        int pageSize = 6;
        int currentPage = Math.Max(1, page);

        var dbCourses = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        List<int> enrolledCourseIds = new();
        if (User.Identity?.IsAuthenticated == true && int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId))
        {
            enrolledCourseIds = await _db.Enrollments
                .Where(e => e.UserId == userId)
                .Select(e => e.CourseId)
                .ToListAsync();
        }

        var courseViewModels = dbCourses.Select(c => new CourseViewModel
        {
            Id = c.Id,
            TitleEn = c.TitleEn,
            TitleBn = c.TitleBn,
            DescriptionEn = c.DescriptionEn,
            DescriptionBn = c.DescriptionBn,
            CategoryEn = c.CategoryEn,
            CategoryBn = c.CategoryBn,
            TargetGen = c.TargetGen,
            TargetGenBn = GetTargetGenBn(c.TargetGen),
            Language = string.IsNullOrWhiteSpace(c.Language) ? "Bangla" : c.Language,
            Duration = c.DurationEn,
            DurationBn = c.DurationBn,
            ModuleCount = c.Modules.Count > 0 ? c.Modules.Count : 4,
            IsFree = c.IsFree,
            IsPayWhatYouWant = c.IsPayWhatYouWant,
            SuggestedFeeBDT = c.Price,
            Rating = Math.Min(5.0, c.Rating > 0 ? c.Rating : 4.9),
            EnrolledCount = c.EnrolledCount > 0 ? c.EnrolledCount : 1250,
            ApprovalStatus = c.ApprovalStatus,
            IsUserEnrolled = enrolledCourseIds.Contains(c.Id),
            Tags = new List<string> { c.CategoryEn, c.TargetGen, string.IsNullOrWhiteSpace(c.Language) ? "Bangla" : c.Language },
            Modules = c.Modules.OrderBy(m => m.StepNumber).Select(m => new CourseModuleItem
            {
                Id = m.Id,
                StepNumber = m.StepNumber,
                TitleEn = m.TitleEn,
                TitleBn = m.TitleBn,
                ShortDescriptionBn = m.ShortDescriptionBn,
                ShortDescriptionEn = m.ShortDescriptionEn,
                Duration = m.DurationEn,
                DurationBn = m.DurationBn,
                TypeEn = m.TypeEn,
                TypeBn = m.TypeBn,
                OptionalMaterials = m.OptionalMaterials
            }).ToList()
        }).ToList();

        var model = new CoursesHubViewModel
        {
            SelectedCategory = category,
            SelectedAgeGroup = ageGroup,
            SearchQuery = search,
            Courses = courseViewModels,
            CategoriesEn = new() { "All", "Cyber Safety", "Personal Safety", "Mental Health", "Parenting & Guidance" },
            CategoriesBn = new() { "সকল", "সাইবার নিরাপত্তা", "ব্যক্তিগত সুরক্ষা", "মানসিক স্বাস্থ্য", "অভিভাবকত্ব ও গাইডেন্স" },
            AgeGroups = new() { "All", "Gen Z & Alpha (10-24y)", "Young Adults (18-25y)", "Parents & Guardians" },
            TotalLearnersCount = 18450,
            CertificatesIssued = 9230,
            CurrentPage = currentPage,
            PageSize = pageSize,
            TotalCoursesCount = totalCourses
        };

        return View("Index", model);
    }

    // Direct My Courses endpoint for enrolled user account dashboard
    [Authorize]
    public async Task<IActionResult> MyCourses()
    {
        int userId = GetUserId();
        var enrollments = await _db.Enrollments
            .Include(e => e.Course)
                .ThenInclude(c => c!.Modules)
            .Where(e => e.UserId == userId)
            .ToListAsync();

        var enrolledCourses = new List<CourseViewModel>();

        foreach (var e in enrollments)
        {
            if (e.Course == null) continue;

            var userReview = await _db.Reviews
                .FirstOrDefaultAsync(r => r.UserId == userId && r.TargetId == e.CourseId && r.TargetType == "Course");

            var allReviews = await _db.Reviews
                .Include(r => r.User)
                .Where(r => r.TargetId == e.CourseId && r.TargetType == "Course")
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .ToListAsync();

            var vm = new CourseViewModel
            {
                Id = e.Course.Id,
                TitleEn = e.Course.TitleEn,
                TitleBn = e.Course.TitleBn,
                DescriptionEn = e.Course.DescriptionEn,
                DescriptionBn = e.Course.DescriptionBn,
                CategoryEn = e.Course.CategoryEn,
                CategoryBn = e.Course.CategoryBn,
                TargetGen = e.Course.TargetGen,
                TargetGenBn = GetTargetGenBn(e.Course.TargetGen),
                Language = string.IsNullOrWhiteSpace(e.Course.Language) ? "Bangla" : e.Course.Language,
                Duration = e.Course.DurationEn,
                DurationBn = e.Course.DurationBn,
                ModuleCount = e.Course.Modules.Count > 0 ? e.Course.Modules.Count : 4,
                CompletedModules = e.CompletedModulesCount,
                IsFree = e.Course.IsFree,
                IsPayWhatYouWant = e.Course.IsPayWhatYouWant,
                SuggestedFeeBDT = e.Course.Price,
                Rating = Math.Min(5.0, e.Course.Rating > 0 ? e.Course.Rating : 4.9),
                IsUserEnrolled = true,
                UserRating = userReview?.Rating,
                UserReviewComment = userReview?.Comment,
                Tags = new List<string> { e.Course.CategoryEn, e.Course.TargetGen },
                Modules = e.Course.Modules.OrderBy(m => m.StepNumber).Select(m => new CourseModuleItem
                {
                    Id = m.Id,
                    StepNumber = m.StepNumber,
                    TitleEn = m.TitleEn,
                    TitleBn = m.TitleBn,
                    ShortDescriptionBn = m.ShortDescriptionBn,
                    ShortDescriptionEn = m.ShortDescriptionEn,
                    Duration = m.DurationEn,
                    DurationBn = m.DurationBn,
                    TypeEn = m.TypeEn,
                    TypeBn = m.TypeBn,
                    ContentMarkdown = m.ContentMarkdown,
                    OptionalMaterials = m.OptionalMaterials,
                    IsCompleted = m.StepNumber <= e.CompletedModulesCount
                }).ToList(),
                Reviews = allReviews.Select(r => new ReviewItemViewModel
                {
                    Id = r.Id,
                    UserName = r.User?.FullName ?? "Anonymous Youth",
                    UserAvatar = r.User?.ProfilePictureUrl ?? "",
                    Rating = Math.Min(5, Math.Max(1, r.Rating)),
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                }).ToList()
            };

            enrolledCourses.Add(vm);
        }

        return View("MyCourses", enrolledCourses);
    }

    // UC-11 / FR-4: Enroll Action (Requires Login)
    public async Task<IActionResult> Enroll(int id)
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            TempData["InfoMessage"] = "কোর্সে এনরোল করতে ও আপনার অগ্রগতি সংরক্ষণ করতে অনুগ্রহ করে একটি অ্যাকাউন্টে লগইন করুন। / Please log in to enroll and track your learning.";
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Enroll", "Courses", new { id }) });
        }

        int userId = GetUserId();

        var course = await _db.Courses.FindAsync(id);
        if (course == null)
        {
            var fallback = HavenDataStore.GetCourses().FirstOrDefault(c => c.Id == id);
            if (fallback != null)
            {
                course = new Course
                {
                    TitleEn = fallback.TitleEn,
                    TitleBn = fallback.TitleBn,
                    CategoryEn = fallback.CategoryEn,
                    CategoryBn = fallback.CategoryBn,
                    DescriptionEn = fallback.DescriptionEn,
                    DescriptionBn = fallback.DescriptionBn,
                    TargetGen = fallback.TargetGen,
                    Language = "Bangla",
                    DurationEn = fallback.Duration,
                    DurationBn = fallback.DurationBn,
                    IsFree = fallback.IsFree,
                    IsPayWhatYouWant = fallback.IsPayWhatYouWant,
                    Price = fallback.SuggestedFeeBDT,
                    ApprovalStatus = "Approved"
                };
                _db.Courses.Add(course);
                await _db.SaveChangesAsync();
            }
            else
            {
                return NotFound();
            }
        }

        var enrollment = await _db.Enrollments.FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == course.Id);
        if (enrollment == null)
        {
            _db.Enrollments.Add(new Enrollment
            {
                UserId = userId,
                CourseId = course.Id,
                CompletedModulesCount = 1,
                ProgressPercentage = 25,
                EnrolledAt = DateTime.UtcNow,
                IsCompleted = false
            });
            course.EnrolledCount += 1;
            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "আপনি সফলভাবে কোর্সটিতে এনরোল হয়েছেন! / You have successfully enrolled in this course!";
        }
        else
        {
            TempData["InfoMessage"] = "আপনি ইতিমধ্যে এই কোর্সে এনরোলকৃত আছেন। / You are already enrolled in this course.";
        }

        return RedirectToAction(nameof(MyCourses));
    }

    // UC-10: Public Course Details & Preview with Full Module Descriptions & Reviews
    public async Task<IActionResult> Details(int id)
    {
        var course = await _db.Courses
            .Include(c => c.Modules)
            .Include(c => c.Author)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (course == null)
        {
            var fallback = HavenDataStore.GetCourses().FirstOrDefault(c => c.Id == id);
            if (fallback == null) return NotFound();

            var vmFallback = new CourseViewModel
            {
                Id = fallback.Id,
                TitleEn = fallback.TitleEn,
                TitleBn = fallback.TitleBn,
                DescriptionEn = fallback.DescriptionEn,
                DescriptionBn = fallback.DescriptionBn,
                CategoryEn = fallback.CategoryEn,
                CategoryBn = fallback.CategoryBn,
                TargetGen = fallback.TargetGen,
                TargetGenBn = GetTargetGenBn(fallback.TargetGen),
                Language = "Bangla",
                Duration = fallback.Duration,
                DurationBn = fallback.DurationBn,
                IsFree = fallback.IsFree,
                IsPayWhatYouWant = fallback.IsPayWhatYouWant,
                SuggestedFeeBDT = fallback.SuggestedFeeBDT,
                Rating = Math.Min(5.0, fallback.Rating),
                EnrolledCount = fallback.EnrolledCount,
                Modules = fallback.Modules.Select(m => new CourseModuleItem
                {
                    StepNumber = m.StepNumber,
                    TitleEn = m.TitleEn,
                    TitleBn = m.TitleBn,
                    ShortDescriptionBn = "মডিউল সেশনের মূল বিষয়বস্তু ও দিকনির্দেশনা।",
                    Duration = m.Duration,
                    DurationBn = m.DurationBn,
                    TypeEn = m.Type,
                    TypeBn = m.TypeBn,
                    OptionalMaterials = "https://haven.org/resources/guide.pdf"
                }).ToList()
            };
            return View(vmFallback);
        }

        bool isEnrolled = false;
        if (User.Identity?.IsAuthenticated == true)
        {
            int userId = GetUserId();
            isEnrolled = await _db.Enrollments.AnyAsync(e => e.UserId == userId && e.CourseId == id);
        }

        // Fetch course reviews
        var reviews = await _db.Reviews
            .Include(r => r.User)
            .Where(r => r.TargetId == id && r.TargetType == "Course")
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReviewItemViewModel
            {
                Id = r.Id,
                UserName = r.User != null ? r.User.FullName : "Anonymous Youth",
                UserAvatar = r.User != null ? r.User.ProfilePictureUrl ?? "" : "",
                Rating = Math.Min(5, Math.Max(1, r.Rating)),
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        var vm = new CourseViewModel
        {
            Id = course.Id,
            AuthorId = course.AuthorId,
            AuthorName = course.Author?.FullName ?? "HAVEN Clinical Team",
            TitleEn = course.TitleEn,
            TitleBn = course.TitleBn,
            DescriptionEn = course.DescriptionEn,
            DescriptionBn = course.DescriptionBn,
            CategoryEn = course.CategoryEn,
            CategoryBn = course.CategoryBn,
            TargetGen = course.TargetGen,
            TargetGenBn = GetTargetGenBn(course.TargetGen),
            Language = string.IsNullOrWhiteSpace(course.Language) ? "Bangla" : course.Language,
            Duration = course.DurationEn,
            DurationBn = course.DurationBn,
            IsFree = course.IsFree,
            IsPayWhatYouWant = course.IsPayWhatYouWant,
            SuggestedFeeBDT = course.Price,
            Rating = Math.Min(5.0, course.Rating > 0 ? course.Rating : 4.9),
            EnrolledCount = course.EnrolledCount,
            ApprovalStatus = course.ApprovalStatus,
            IsUserEnrolled = isEnrolled,
            Tags = new List<string> { course.CategoryEn, course.TargetGen, string.IsNullOrWhiteSpace(course.Language) ? "Bangla" : course.Language },
            Modules = course.Modules.OrderBy(m => m.StepNumber).Select(m => new CourseModuleItem
            {
                Id = m.Id,
                StepNumber = m.StepNumber,
                TitleEn = m.TitleEn,
                TitleBn = m.TitleBn,
                ShortDescriptionBn = m.ShortDescriptionBn,
                ShortDescriptionEn = m.ShortDescriptionEn,
                Duration = m.DurationEn,
                DurationBn = m.DurationBn,
                TypeEn = m.TypeEn,
                TypeBn = m.TypeBn,
                ContentMarkdown = isEnrolled ? m.ContentMarkdown : "",
                OptionalMaterials = m.OptionalMaterials
            }).ToList(),
            Reviews = reviews
        };

        return View(vm);
    }

    // FR-9: Therapist/Clinician Course Creation Portal
    [HttpGet]
    [Authorize]
    public IActionResult Create()
    {
        if (!User.IsInRole("Professional") && !User.IsInRole("Admin"))
        {
            TempData["ErrorMessage"] = "শুধুমাত্র নিবন্ধিত থেরাপিস্ট ও অ্যাডমিনগণ কোর্স তৈরি করতে পারবেন। / Only verified therapists and admins can publish safety courses.";
            return RedirectToAction("Index");
        }

        var model = new CreateCourseViewModel();
        return View(model);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCourseViewModel model)
    {
        if (!User.IsInRole("Professional") && !User.IsInRole("Admin"))
        {
            return RedirectToAction("Index");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        int userId = GetUserId();
        bool isAdmin = User.IsInRole("Admin");

        var course = new Course
        {
            AuthorId = userId,
            TitleEn = model.TitleEn.Trim(),
            TitleBn = string.IsNullOrWhiteSpace(model.TitleBn) ? model.TitleEn.Trim() : model.TitleBn.Trim(),
            CategoryEn = model.CategoryEn,
            CategoryBn = string.IsNullOrWhiteSpace(model.CategoryBn) ? model.CategoryEn : model.CategoryBn,
            DescriptionEn = model.DescriptionEn.Trim(),
            DescriptionBn = string.IsNullOrWhiteSpace(model.DescriptionBn) ? model.DescriptionEn.Trim() : model.DescriptionBn.Trim(),
            TargetGen = model.TargetGen,
            Language = string.IsNullOrWhiteSpace(model.Language) ? "Bangla" : model.Language,
            DurationEn = model.Duration,
            DurationBn = model.Duration,
            IsFree = model.IsFree,
            IsPayWhatYouWant = model.IsPayWhatYouWant,
            Price = model.SuggestedFeeBDT,
            Rating = 5.0,
            EnrolledCount = 0,
            ApprovalStatus = isAdmin ? "Approved" : "Pending",
            CreatedAt = DateTime.UtcNow
        };

        _db.Courses.Add(course);
        await _db.SaveChangesAsync();

        if (model.Modules != null && model.Modules.Any())
        {
            foreach (var m in model.Modules)
            {
                _db.CourseModules.Add(new CourseModule
                {
                    CourseId = course.Id,
                    StepNumber = m.StepNumber,
                    TitleEn = m.TitleEn.Trim(),
                    TitleBn = string.IsNullOrWhiteSpace(m.TitleBn) ? m.TitleEn.Trim() : m.TitleBn.Trim(),
                    ShortDescriptionBn = m.ShortDescriptionBn?.Trim() ?? "",
                    ShortDescriptionEn = m.ShortDescriptionEn?.Trim() ?? "",
                    DurationEn = m.Duration,
                    DurationBn = m.Duration,
                    ContentMarkdown = m.ContentMarkdown,
                    OptionalMaterials = m.OptionalMaterials?.Trim() ?? ""
                });
            }
            await _db.SaveChangesAsync();
        }

        if (isAdmin)
        {
            TempData["SuccessMessage"] = "কোর্সটি সফলভাবে তৈরি ও প্রকাশিত হয়েছে! / Course published successfully!";
        }
        else
        {
            TempData["SuccessMessage"] = "আপনার কোর্সটি পর্যালোচনার জন্য অ্যাডমিন প্যানেলে জমা দেওয়া হয়েছে। / Your course has been submitted for admin approval.";
        }

        return RedirectToAction("Index");
    }

    // Submit Review & Star Rating (1 to 5 Stars Max)
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitReview(int courseId, int rating, string comment)
    {
        rating = Math.Min(5, Math.Max(1, rating));
        int userId = GetUserId();

        var enrollment = await _db.Enrollments.FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);
        if (enrollment == null)
        {
            TempData["ErrorMessage"] = "রিভিউ দিতে প্রথমে কোর্সটিতে এনরোল করুন। / You must be enrolled to submit a review.";
            return RedirectToAction(nameof(MyCourses));
        }

        var existingReview = await _db.Reviews
            .FirstOrDefaultAsync(r => r.UserId == userId && r.TargetId == courseId && r.TargetType == "Course");

        if (existingReview != null)
        {
            existingReview.Rating = rating;
            existingReview.Comment = comment?.Trim() ?? "";
            existingReview.CreatedAt = DateTime.UtcNow;
        }
        else
        {
            _db.Reviews.Add(new Review
            {
                UserId = userId,
                TargetId = courseId,
                TargetType = "Course",
                Rating = rating,
                Comment = comment?.Trim() ?? "",
                CreatedAt = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync();

        var course = await _db.Courses.FindAsync(courseId);
        if (course != null)
        {
            var allRatings = await _db.Reviews
                .Where(r => r.TargetId == courseId && r.TargetType == "Course")
                .Select(r => r.Rating)
                .ToListAsync();

            if (allRatings.Any())
            {
                double avg = allRatings.Average();
                course.Rating = Math.Min(5.0, Math.Max(1.0, Math.Round(avg, 1)));
                await _db.SaveChangesAsync();
            }
        }

        TempData["SuccessMessage"] = "আপনার মূল্যবান রিভিউ ও রেটিং যুক্ত হয়েছে! / Thank you! Your review and rating have been recorded.";
        return RedirectToAction(nameof(MyCourses));
    }

    // UC-12: Module Progress Checkbox (Requires Authenticated Session)
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> ToggleModuleProgress(int courseId, int stepNumber, bool isCompleted)
    {
        int userId = GetUserId();
        var enrollment = await _db.Enrollments
            .Include(e => e.Course)
                .ThenInclude(c => c!.Modules)
            .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);

        if (enrollment != null)
        {
            int totalModules = enrollment.Course?.Modules.Count ?? 4;
            if (isCompleted)
            {
                enrollment.CompletedModulesCount = Math.Min(totalModules, enrollment.CompletedModulesCount + 1);
            }
            else
            {
                enrollment.CompletedModulesCount = Math.Max(0, enrollment.CompletedModulesCount - 1);
            }

            enrollment.ProgressPercentage = totalModules > 0 ? (int)((double)enrollment.CompletedModulesCount / totalModules * 100) : 0;
            enrollment.IsCompleted = enrollment.ProgressPercentage >= 100;
            if (enrollment.IsCompleted && enrollment.CompletedAt == null)
            {
                enrollment.CompletedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();
        }

        return Json(new
        {
            success = true,
            courseId,
            stepNumber,
            isCompleted,
            progressPercentage = enrollment?.ProgressPercentage ?? 0,
            completedModulesCount = enrollment?.CompletedModulesCount ?? 0,
            message = isCompleted ? "মডিউল সম্পন্ন হিসেবে চিহ্নিত হয়েছে!" : "মডিউল আবার সক্রিয় করা হয়েছে।"
        });
    }

    private int GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out int id) ? id : 0;
    }

    private static string GetTargetGenBn(string targetGen)
    {
        return targetGen switch
        {
            "Gen Z & Alpha (10-24y)" => "জেন জি ও আলফা (১০-২৪ বছর)",
            "Young Adults (18-25y)" => "তরুণ বয়স্ক (১৮-২৫ বছর)",
            "Parents & Guardians" => "অভিভাবক ও অভিভাবিকা",
            _ => "জেন জি ও আলফা"
        };
    }

    private async Task SeedInitialCoursesIfEmptyAsync()
    {
        if (!await _db.Courses.AnyAsync())
        {
            var defaults = HavenDataStore.GetCourses();
            foreach (var d in defaults)
            {
                var c = new Course
                {
                    TitleEn = d.TitleEn,
                    TitleBn = d.TitleBn,
                    CategoryEn = d.CategoryEn,
                    CategoryBn = d.CategoryBn,
                    DescriptionEn = d.DescriptionEn,
                    DescriptionBn = d.DescriptionBn,
                    TargetGen = d.TargetGen,
                    Language = "Bangla",
                    DurationEn = d.Duration,
                    DurationBn = d.DurationBn,
                    IsFree = d.IsFree,
                    IsPayWhatYouWant = d.IsPayWhatYouWant,
                    Price = d.SuggestedFeeBDT,
                    Rating = d.Rating,
                    EnrolledCount = d.EnrolledCount,
                    ApprovalStatus = "Approved",
                    CreatedAt = DateTime.UtcNow
                };

                _db.Courses.Add(c);
                await _db.SaveChangesAsync();

                int step = 1;
                foreach (var m in d.Modules)
                {
                    _db.CourseModules.Add(new CourseModule
                    {
                        CourseId = c.Id,
                        StepNumber = step++,
                        TitleEn = m.TitleEn,
                        TitleBn = m.TitleBn,
                        ShortDescriptionBn = "ডিজিটাল সুরক্ষা ও প্র্যাকটিক্যাল নির্দেশিকা।",
                        ShortDescriptionEn = "Practical digital safety & resilience guide.",
                        DurationEn = m.Duration,
                        DurationBn = m.DurationBn,
                        ContentMarkdown = "### Interactive Module Lesson\n\nPreserve digital evidence safely.",
                        OptionalMaterials = "https://haven.org/resources/digital-safety.pdf"
                    });
                }
                await _db.SaveChangesAsync();
            }
        }
    }
}
