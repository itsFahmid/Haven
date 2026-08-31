using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Haven.Models;

[Table("ProfessionalProfiles")]
public class ProfessionalProfile
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    [Required]
    [MaxLength(200)]
    public string TitleBn { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string TitleEn { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Specialty { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LicenseNo { get; set; } = string.Empty;

    [MaxLength(500)]
    public string LicenseDocumentUrl { get; set; } = string.Empty;

    [MaxLength(50)]
    public string ApprovalStatus { get; set; } = "Pending"; // Pending, Approved, Rejected

    [Column(TypeName = "decimal(18,2)")]
    public decimal HourlyRateBDT { get; set; } = 0;

    public bool IsBmdcVerified { get; set; } = false;

    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    public DateTime? VerifiedAt { get; set; }
}
