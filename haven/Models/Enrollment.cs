using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Haven.Models;

[Table("Enrollments")]
public class Enrollment
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    [Required]
    public int CourseId { get; set; }

    [ForeignKey(nameof(CourseId))]
    public Course? Course { get; set; }

    public int CompletedModulesCount { get; set; } = 0;

    public int ProgressPercentage { get; set; } = 0;

    public bool IsCompleted { get; set; } = false;

    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAt { get; set; }
}
