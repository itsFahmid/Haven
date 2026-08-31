using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Haven.Models;

[Table("ChildProfiles")]
public class ChildProfile
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ParentUserId { get; set; }

    [ForeignKey(nameof(ParentUserId))]
    public User? ParentUser { get; set; }

    [Required]
    [MaxLength(100)]
    public string AliasName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string AgeGroup { get; set; } = "Child"; // Gen Alpha, Gen Beta, Child, Teen

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
