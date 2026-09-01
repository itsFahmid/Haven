using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Haven.Models;

[Table("Courses")]
public class Course
{
    [Key]
    public int Id { get; set; }

    public int? AuthorId { get; set; }

    [ForeignKey(nameof(AuthorId))]
    public User? Author { get; set; }

    [Required]
    [MaxLength(200)]
    public string TitleBn { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string TitleEn { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string CategoryBn { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string CategoryEn { get; set; } = string.Empty;

    public string DescriptionBn { get; set; } = string.Empty;

    public string DescriptionEn { get; set; } = string.Empty;

    [MaxLength(50)]
    public string DurationBn { get; set; } = "৩০ মিনিট";

    [MaxLength(50)]
    public string DurationEn { get; set; } = "30 Mins";

    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; } = 0;

    public bool IsFree { get; set; } = true;

    public bool IsPayWhatYouWant { get; set; } = false;

    [MaxLength(50)]
    public string TargetGen { get; set; } = "Gen Z & Alpha";

    [MaxLength(50)]
    public string Language { get; set; } = "Bangla"; // Bangla, English, Bilingual

    public double Rating { get; set; } = 4.9;

    public int EnrolledCount { get; set; } = 0;

    [MaxLength(50)]
    public string ApprovalStatus { get; set; } = "Approved"; // Pending, Approved, Rejected

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<CourseModule> Modules { get; set; } = new();
}
