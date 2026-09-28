namespace Obhoy.Models;

public class PaymentViewModel
{
    public string Purpose { get; set; } = "Clinical Care Subsidy & Infrastructure";
    public string PurposeBn { get; set; } = "মানসিক সেবা ভর্তুকি ও উন্মুক্ত অবকাঠামো";
    public int AmountBDT { get; set; } = 600; // Default to 1 full subsidized clinical session
    public string SelectedGateway { get; set; } = "sslcommerz"; // "sslcommerz", "bkash", "nagad", "rocket"
    public bool OptIntoHallOfFame { get; set; } = false;
    public string? DonorDisplayName { get; set; }
    public string? RecognitionMessage { get; set; }
    public string? MobileNumber { get; set; }
    public string? TransactionId { get; set; }
    public bool IsAnonymous { get; set; } = true;

    // Aggregate Anonymous Stewardship Telemetry
    public decimal TotalSponsoredPoolBDT { get; set; } = 58400;
    public int SubsidizedSessionsCount { get; set; } = 74;
    public int HelplineUptimeHours { get; set; } = 720;
    public int ProtectedYouthCount { get; set; } = 28490;
    public int VerifiedCliniciansCount { get; set; } = 38;

    public List<HallOfFameDonor> HallOfFameDonors { get; set; } = new();
}

public class HallOfFameDonor
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal AmountBDT { get; set; }
    public string BadgeEn { get; set; } = "Care Sustainer";
    public string BadgeBn { get; set; } = "সেবা সহযোগী";
    public string TimeAgoEn { get; set; } = "Recently";
    public string TimeAgoBn { get; set; } = "সম্প্রতি";
    public string City { get; set; } = "Bangladesh";
    public string? Message { get; set; }
    public DateTime CreatedAt { get; set; }
}
