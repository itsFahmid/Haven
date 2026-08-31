using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Haven.Models;

[Table("Reviews")]
public class Review
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    public int TargetId { get; set; }

    [MaxLength(50)]
    public string TargetType { get; set; } = "Course"; // Course, Therapist

    public int Rating { get; set; } = 5;

    public string Comment { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
