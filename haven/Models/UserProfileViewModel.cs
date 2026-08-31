namespace Haven.Models;

public class UserProfileViewModel
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public int CompletedCoursesCount { get; set; } = 2;
    public int BookedSessionsCount { get; set; } = 1;
}
