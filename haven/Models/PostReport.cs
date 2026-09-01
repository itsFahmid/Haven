using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Haven.Models;

[Table("PostReports")]
public class PostReport
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int PostId { get; set; }

    [ForeignKey(nameof(PostId))]
    public CommunityPost? Post { get; set; }

    public int? ReporterUserId { get; set; }

    [ForeignKey(nameof(ReporterUserId))]
    public User? Reporter { get; set; }

    [Required]
    [MaxLength(250)]
    public string Reason { get; set; } = "Inappropriate Content";

    public DateTime ReportedAt { get; set; } = DateTime.UtcNow;
}
