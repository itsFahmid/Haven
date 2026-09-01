using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Haven.Models;

[Table("ArticleBookmarks")]
public class ArticleBookmark
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    [Required]
    public int ArticleId { get; set; }

    [ForeignKey(nameof(ArticleId))]
    public Article? Article { get; set; }

    public DateTime BookmarkedAt { get; set; } = DateTime.UtcNow;
}
