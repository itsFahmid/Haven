using System.ComponentModel.DataAnnotations;

namespace Obhoy.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Email is required / à¦‡à¦®à§‡à¦‡à¦² à¦ à¦¿à¦•à¦¾à¦¨à¦¾ à¦†à¦¬à¦¶à§à¦¯à¦•")]
    [EmailAddress(ErrorMessage = "Invalid email format / à¦¸à¦ à¦¿à¦• à¦‡à¦®à§‡à¦‡à¦² à¦¦à¦¿à¦¨")]
    [Display(Name = "Email Address / à¦‡à¦®à§‡à¦‡à¦²")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required / à¦ªà¦¾à¦¸à¦“à¦¯à¦¼à¦¾à¦°à§à¦¡ à¦ªà§à¦°à¦¦à¦¾à¦¨ à¦•à¦°à§à¦¨")]
    [DataType(DataType.Password)]
    [Display(Name = "Password / à¦ªà¦¾à¦¸à¦“à¦¯à¦¼à¦¾à¦°à§à¦¡")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember Me / à¦®à¦¨à§‡ à¦°à¦¾à¦–à§à¦¨")]
    public bool RememberMe { get; set; } = false;

    public string? ReturnUrl { get; set; }
}
