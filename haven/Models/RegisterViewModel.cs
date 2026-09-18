using System.ComponentModel.DataAnnotations;

namespace Obhoy.Models;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Full name is required / à¦†à¦ªà¦¨à¦¾à¦° à¦ªà§‚à¦°à§à¦£ à¦¨à¦¾à¦® à¦†à¦¬à¦¶à§à¦¯à¦•")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
    [Display(Name = "Full Name / à¦ªà§‚à¦°à§à¦£ à¦¨à¦¾à¦®")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email address is required / à¦‡à¦®à§‡à¦‡à¦² à¦ à¦¿à¦•à¦¾à¦¨à¦¾ à¦†à¦¬à¦¶à§à¦¯à¦•")]
    [EmailAddress(ErrorMessage = "Invalid email address format / à¦¸à¦ à¦¿à¦• à¦‡à¦®à§‡à¦‡à¦² à¦ªà§à¦°à¦¦à¦¾à¦¨ à¦•à¦°à§à¦¨")]
    [StringLength(150)]
    [Display(Name = "Email Address / à¦‡à¦®à§‡à¦‡à¦²")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Account Mode selection is required")]
    public string UserType { get; set; } = "Individual"; // Individual or Parent

    [Range(10, 120, ErrorMessage = "Please enter a valid age")]
    public int? Age { get; set; }

    [Required(ErrorMessage = "Password is required / à¦ªà¦¾à¦¸à¦“à¦¯à¦¼à¦¾à¦°à§à¦¡ à¦†à¦¬à¦¶à§à¦¯à¦•")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters / à¦ªà¦¾à¦¸à¦“à¦¯à¦¼à¦¾à¦°à§à¦¡ à¦¨à§à¦¯à§‚à¦¨à¦¤à¦® à§¬ à¦…à¦•à§à¦·à¦°à§‡à¦° à¦¹à¦¤à§‡ à¦¹à¦¬à§‡")]
    [DataType(DataType.Password)]
    [Display(Name = "Password / à¦ªà¦¾à¦¸à¦“à¦¯à¦¼à¦¾à¦°à§à¦¡")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm your password / à¦ªà¦¾à¦¸à¦“à¦¯à¦¼à¦¾à¦°à§ à¦¡ à¦¨à¦¿à¦¶à§ à¦šà¦¿à¦¤ à¦•à¦°à§ à¦¨")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Passwords do not match / à¦¦à§ à¦Ÿà¦¿ à¦ªà¦¾à¦¸à¦“à¦¯à¦¼à¦¾à¦°à§ à¦¡ à¦®à¦¿à¦²à¦›à§‡ à¦¨à¦¾")]
    [Display(Name = "Confirm Password / à¦ªà§ à¦¨à¦°à¦¾à¦¯à¦¼ à¦ªà¦¾à¦¸à¦“à¦¯à¦¼à¦¾à¦°à§ à¦¡")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Range(typeof(bool), "true", "true", ErrorMessage = "You must accept the Obhoy safety and privacy terms / শর্তাবলীতে সম্মতি প্রদান করুন")]
    [Display(Name = "I agree to Obhoy Safe Space and Privacy Terms")]
    public bool AgreeToTerms { get; set; } = true;

    public string? ReturnUrl { get; set; }
}
