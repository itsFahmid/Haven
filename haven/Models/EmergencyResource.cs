using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Haven.Models;

[Table("EmergencyResources")]
public class EmergencyResource
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string NameBn { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string NameEn { get; set; } = string.Empty;

    [MaxLength(100)]
    public string District { get; set; } = "National";

    [Required]
    [MaxLength(50)]
    public string ContactNumber { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Type { get; set; } = "NationalHelpline"; // NationalHelpline, ChildProtection, LegalAid, Shelter

    public bool IsTollFree { get; set; } = true;
}
