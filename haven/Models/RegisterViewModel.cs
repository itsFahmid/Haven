using System.ComponentModel.DataAnnotations;

namespace Haven.Models;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Full name is required / আপনার পূর্ণ নাম আবশ্যক")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
    [Display(Name = "Full Name / পূর্ণ নাম")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email address is required / ইমেইল ঠিকানা আবশ্যক")]
    [EmailAddress(ErrorMessage = "Invalid email address format / সঠিক ইমেইল প্রদান করুন")]
    [StringLength(150)]
    [Display(Name = "Email Address / ইমেইল")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Account Mode selection is required")]
    public string UserType { get; set; } = "Individual"; // Individual or Parent

    [Range(10, 120, ErrorMessage = "Please enter a valid age")]
    public int? Age { get; set; }

    [Required(ErrorMessage = "Password is required / পাসওয়ার্ড আবশ্যক")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters / পাসওয়ার্ড ন্যূনতম ৬ অক্ষরের হতে হবে")]
    [DataType(DataType.Password)]
    [Display(Name = "Password / পাসওয়ার্ড")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm your password / পাসওয়ার্ড নিশ্চিত করুন")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Passwords do not match / দুটি পাসওয়ার্ড মিলছে না")]
    [Display(Name = "Confirm Password / পুনরায় পাসওয়ার্ড")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Range(typeof(bool), "true", "true", ErrorMessage = "You must accept the Haven safety and privacy terms / শর্তাবলীতে সম্মতি প্রদান করুন")]
    [Display(Name = "I agree to Haven Safe Space and Privacy Terms")]
    public bool AgreeToTerms { get; set; } = true;

    public string? ReturnUrl { get; set; }
}
