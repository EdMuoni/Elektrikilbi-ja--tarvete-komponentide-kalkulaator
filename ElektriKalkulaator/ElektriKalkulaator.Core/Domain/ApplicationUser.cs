using Microsoft.AspNetCore.Identity;

namespace ElektriKalkulaator.Core.Domain
{
    // Represents a person who can log in.
    //
    // It inherits from IdentityUser, which is ASP.NET Core Identity's built-in user type. That
    // base class already provides everything security-critical and easy to get wrong: the email
    // and username, a securely hashed password (never the password itself), email confirmation,
    // failed-login lockout, and two-factor support.
    //
    // We inherit rather than write our own user table because password storage is exactly the
    // kind of code that should not be hand-rolled.
    //
    // Anything specific to this application goes below as extra properties.
    public class ApplicationUser : IdentityUser
    {
        // Shown in the navigation bar instead of the email address. Optional.
        public string? FullName { get; set; }

        // Preferred interface language: "et", "en" or "ru".
        // The ERD specification (ERD_Loogiline_Seletus.docx) defines this field on USER. The UI is
        // Estonian-only for now, so nothing reads it yet — it exists so the planned multilingual
        // work has somewhere to store the choice.
        public string Language { get; set; } = "et";

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    // The two roles the application recognises.
    //
    // These are plain string constants rather than loose text scattered through the code, so a
    // typo like [Authorize(Roles = "Admn")] becomes a compile error instead of a silent security
    // hole that lets everyone through.
    public static class UserRoles
    {
        // Can manage the product catalogue and categories.
        public const string Admin = "Admin";

        // A registered customer: can use the calculator and the cart, but not manage products.
        public const string Customer = "Customer";
    }
}
