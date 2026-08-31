using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Haven.Models;

[Table("Users")]
public class User
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Role { get; set; } = "User"; // Admin, Professional, User

    [MaxLength(50)]
    public string UserType { get; set; } = "Individual"; // Individual, Parent

    public int? Age { get; set; }

    public bool IsAnonymous { get; set; } = false;

    [MaxLength(500)]
    public string? ProfilePictureUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastLoginAt { get; set; }

    public bool IsActive { get; set; } = true;
}
