using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Haven.Models;

[Table("CommunityComments")]
public class CommunityComment
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int PostId { get; set; }

    [ForeignKey(nameof(PostId))]
    public CommunityPost? Post { get; set; }

    public int? UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    [Required]
    public string CommentText { get; set; } = string.Empty;

    public bool IsAnonymous { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
