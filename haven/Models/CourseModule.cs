using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Haven.Models;

[Table("CourseModules")]
public class CourseModule
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int CourseId { get; set; }

    [ForeignKey(nameof(CourseId))]
    public Course? Course { get; set; }

    public int StepNumber { get; set; }

    [Required]
    [MaxLength(200)]
    public string TitleBn { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string TitleEn { get; set; } = string.Empty;

    [MaxLength(100)]
    public string TypeBn { get; set; } = "পাঠ ও কুইজ";

    [MaxLength(100)]
    public string TypeEn { get; set; } = "Lesson & Quiz";

    [MaxLength(50)]
    public string DurationBn { get; set; } = "১০ মিনিট";

    [MaxLength(50)]
    public string DurationEn { get; set; } = "10 Mins";

    [MaxLength(500)]
    public string ShortDescriptionBn { get; set; } = string.Empty;

    [MaxLength(500)]
    public string ShortDescriptionEn { get; set; } = string.Empty;

    public string ContentMarkdown { get; set; } = string.Empty;

    public string OptionalMaterials { get; set; } = string.Empty; // Resource links, downloadable materials
}
