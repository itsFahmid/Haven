namespace Haven.Models;

public class TherapistDashboardViewModel
{
    public ProfessionalProfile Therapist { get; set; } = null!;
    public List<Booking> Requests { get; set; } = new();
    public string CurrentFilter { get; set; } = "All"; // All, Pending, Approved, Rejected

    public int TotalRequestsCount { get; set; }
    public int PendingRequestsCount { get; set; }
    public int ApprovedRequestsCount { get; set; }
    public int RejectedRequestsCount { get; set; }
}
