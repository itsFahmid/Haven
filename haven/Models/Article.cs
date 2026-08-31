using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Haven.Models;

[Table("Articles")]
public class Article
{
    [Key]
    public int Id { get; set; }

    public int? AuthorId { get; set; }

    [ForeignKey(nameof(AuthorId))]
    public User? Author { get; set; }

    [Required]
    [MaxLength(250)]
    public string TitleBn { get; set; } = string.Empty;

    [Required]
    [MaxLength(250)]
    public string TitleEn { get; set; } = string.Empty;

    public string ContentMarkdown { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Category { get; set; } = "Grooming Prevention";

    [MaxLength(50)]
    public string ApprovalStatus { get; set; } = "Approved"; // Pending, Approved, Rejected

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
