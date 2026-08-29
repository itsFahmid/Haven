namespace Haven.Models;

public class HomeViewModel
{
    public List<EmergencyContact> Hotlines { get; set; } = new();
    public List<CourseViewModel> FeaturedCourses { get; set; } = new();
    public List<TherapistViewModel> FeaturedTherapists { get; set; } = new();
    public List<HallOfFameDonor> HallOfFameDonors { get; set; } = new();
    public int ActiveYouthProtected { get; set; } = 28490;
    public int CrisesDeescalated { get; set; } = 4120;
    public int VerifiedTherapistsCount { get; set; } = 38;
}
