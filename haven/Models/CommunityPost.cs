using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Haven.Models;

[Table("CommunityPosts")]
public class CommunityPost
{
    [Key]
    public int Id { get; set; }

    public int? UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    [Required]
    [MaxLength(250)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Category { get; set; } = "Experience"; // Story, Experience, Advice, General

    public bool IsAnonymous { get; set; } = true;

    public int LikeCount { get; set; } = 0;

    public int ReportCount { get; set; } = 0;

    public bool IsReported { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<CommunityComment> Comments { get; set; } = new();
}
