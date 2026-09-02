namespace Haven.Models;

public class UserProfileViewModel
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
    public string UserType { get; set; } = "Individual";
    public int? Age { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public int CompletedCoursesCount { get; set; }
    public int EnrolledCoursesCount { get; set; }
    public int BookedSessionsCount { get; set; }
    public List<ChildProfile> ChildProfiles { get; set; } = new();
    public List<Article> BookmarkedArticles { get; set; } = new();
}
