using System.ComponentModel.DataAnnotations;

namespace ElektriKalkulaator.Core.Dto
{
    // What the login form sends. The [Required] and [EmailAddress] attributes are checked
    // automatically by ASP.NET before our controller code runs.
    public class LoginDto
    {
        [Required(ErrorMessage = "E-post on kohustuslik")]
        [EmailAddress(ErrorMessage = "Vigane e-posti aadress")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Parool on kohustuslik")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        // "Keep me signed in" — when false the login cookie is discarded when the browser closes.
        public bool RememberMe { get; set; }
    }

    // What the registration form sends.
    public class RegisterDto
    {
        [Required(ErrorMessage = "E-post on kohustuslik")]
        [EmailAddress(ErrorMessage = "Vigane e-posti aadress")]
        public string Email { get; set; } = "";

        public string? FullName { get; set; }

        [Required(ErrorMessage = "Parool on kohustuslik")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Parool peab olema vähemalt 8 tähemärki")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        // Compare checks this matches Password, so a typo in a password the user cannot see
        // doesn't lock them out of the account they just created.
        [Required(ErrorMessage = "Parooli kinnitus on kohustuslik")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Paroolid ei kattu")]
        public string ConfirmPassword { get; set; } = "";
    }
}
