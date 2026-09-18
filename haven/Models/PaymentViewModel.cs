namespace Obhoy.Models;

public class PaymentViewModel
{
    public string Purpose { get; set; } = "Micro-Donation"; // "Micro-Donation", "Subsidize A Youth Session", "Course Fee"
    public string PurposeBn { get; set; } = "à¦•à§à¦·à§à¦¦à§à¦° à¦…à¦¨à§à¦¦à¦¾à¦¨";
    public int AmountBDT { get; set; } = 100;
    public string SelectedGateway { get; set; } = "bkash"; // "bkash", "nagad", "rocket", "sslcommerz"
    public bool OptIntoHallOfFame { get; set; } = false;
    public string? DonorDisplayName { get; set; }
    public string? MobileNumber { get; set; }
    public string? TransactionId { get; set; }
    public bool IsAnonymous { get; set; } = true;
    public List<HallOfFameDonor> RecentDonors { get; set; } = new();
}

public class HallOfFameDonor
{
    public string Name { get; set; } = string.Empty;
    public int AmountBDT { get; set; }
    public string BadgeEn { get; set; } = "Youth Angel";
    public string BadgeBn { get; set; } = "à¦¤à¦¾à¦°à§à¦£à§à¦¯à§‡à¦° à¦¦à§‚à¦¤";
    public string TimeAgoEn { get; set; } = "2 hours ago";
    public string TimeAgoBn { get; set; } = "à§¨ à¦˜à¦£à§à¦Ÿà¦¾ à¦†à¦—à§‡";
    public string City { get; set; } = "Dhaka";
}
