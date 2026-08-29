using Microsoft.AspNetCore.Mvc;
using Haven.Models;
using Haven.Services;

namespace Haven.Controllers;

public class CoursesController : Controller
{
    public IActionResult Index(string category = "All")
    {
        var allCourses = HavenDataStore.GetCourses();
        var filteredCourses = allCourses;

        if (!string.IsNullOrEmpty(category) && category != "All")
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

        return View(model);
    }

    public IActionResult Details(int id)
    {
        var course = HavenDataStore.GetCourses().FirstOrDefault(c => c.Id == id);
        if (course == null)
        {
            return NotFound();
        }

        return View(course);
    }

    [HttpPost]
    public IActionResult ToggleModuleProgress(int courseId, int stepNumber, bool isCompleted)
    {
        return Json(new { success = true, courseId, stepNumber, isCompleted });
    }
}
