using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Haven.Models;
using Haven.Services;

namespace Haven.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        var model = new HomeViewModel
        {
            Hotlines = HavenDataStore.GetEmergencyHotlines(),
            FeaturedCourses = HavenDataStore.GetCourses().Take(3).ToList(),
            FeaturedTherapists = HavenDataStore.GetTherapists().Take(3).ToList(),
            HallOfFameDonors = HavenDataStore.GetRecentDonors(),
            ActiveYouthProtected = 28490,
            CrisesDeescalated = 4120,
            VerifiedTherapistsCount = 38
        };

        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [HttpPost]
    public IActionResult SetLanguage(string lang, string returnUrl = "/")
    {
        if (lang == "en" || lang == "bn")
        {
            HttpContext.Session.SetString("Haven_Lang", lang);
            Response.Cookies.Append("Haven_Lang", lang, new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true
            });
        }

        if (Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(nameof(Index));
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
