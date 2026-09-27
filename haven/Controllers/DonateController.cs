using Microsoft.AspNetCore.Mvc;
using Obhoy.Models;
using Obhoy.Services;

namespace Obhoy.Controllers;

public class DonateController : Controller
{
    private readonly Obhoy.Data.ObhoyDbContext _db;

    public DonateController(Obhoy.Data.ObhoyDbContext db)
    {
        _db = db;
    }

    public IActionResult Index()
    {
        var realDonors = _db.Payments
            .Where(p => p.Status == PaymentStatus.Completed && p.OptInHallOfFame && !string.IsNullOrEmpty(p.DisplayName))
            .OrderByDescending(p => p.VerifiedAt ?? p.CreatedAt)
            .Take(6)
            .Select(p => new HallOfFameDonor
            {
                Name = p.DisplayName ?? "Kind Supporter",
                AmountBDT = (int)p.Amount,
                BadgeEn = p.Amount >= 5000 ? "Guardian Angel" : "Youth Protector",
                BadgeBn = p.Amount >= 5000 ? "অভিভাবক দূত" : "তরুণদের রক্ষক",
                TimeAgoEn = "Recently",
                TimeAgoBn = "সম্প্রতি",
                City = p.City ?? "Bangladesh"
            })
            .ToList();

        var displayDonors = realDonors.Any() ? realDonors : ObhoyDataStore.GetRecentDonors();

        var model = new PaymentViewModel
        {
            RecentDonors = displayDonors
        };
        return View(model);
    }

    [HttpPost]
    public IActionResult ProcessPayment([FromBody] DonationSubmission submission)
    {
        if (submission == null || submission.AmountBDT <= 0)
        {
            return BadRequest(new { success = false, message = "Invalid donation amount." });
        }

        var trxId = "TXN" + Random.Shared.Next(10000000, 99999999);
        var donorName = submission.IsAnonymous ? "Anonymous Hero" : (string.IsNullOrWhiteSpace(submission.DonorName) ? "Kind Supporter" : submission.DonorName);

        return Json(new
        {
            success = true,
            transactionId = trxId,
            amount = submission.AmountBDT,
            gateway = submission.Gateway,
            donorName = donorName,
            optedIntoHallOfFame = submission.OptIntoHallOfFame,
            messageEn = $"Thank you for your generous contribution of ৳{submission.AmountBDT}! Your support keeps Obhoy completely free for vulnerable youth.",
            messageBn = $"আপনার ৳{submission.AmountBDT} উদার অনুদানের জন্য আন্তরিক ধন্যবাদ! আপনার এই সহযোগিতা বিপদগ্রস্ত তরুণ-কিশোরদের জন্য হেভেনকে উন্মুক্ত রাখতে সাহায্য করবে।"
        });
    }
}

public class DonationSubmission
{
    public int AmountBDT { get; set; } = 100;
    public string Gateway { get; set; } = "bkash";
    public string? DonorName { get; set; }
    public string? MobileNumber { get; set; }
    public bool IsAnonymous { get; set; } = true;
    public bool OptIntoHallOfFame { get; set; }
    public string Purpose { get; set; } = "Micro-Donation";
}
