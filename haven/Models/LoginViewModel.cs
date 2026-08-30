using System.ComponentModel.DataAnnotations;

namespace Haven.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Email is required / ইমেইল ঠিকানা আবশ্যক")]
    [EmailAddress(ErrorMessage = "Invalid email format / সঠিক ইমেইল দিন")]
    [Display(Name = "Email Address / ইমেইল")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required / পাসওয়ার্ড প্রদান করুন")]
    [DataType(DataType.Password)]
    [Display(Name = "Password / পাসওয়ার্ড")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember Me / মনে রাখুন")]
    public bool RememberMe { get; set; } = false;

    public string? ReturnUrl { get; set; }
}
