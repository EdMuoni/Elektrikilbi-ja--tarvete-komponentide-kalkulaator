using ElektriKalkulaator.Core.Domain;
using ElektriKalkulaator.Core.Dto;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ElektriKalkulaator.Controllers
{
    // Registration, login and logout.
    //
    // The actual security work is done by ASP.NET Core Identity's two managers, which are handed
    // to us through the constructor:
    //   UserManager   - creates users, hashes passwords, assigns roles
    //   SignInManager - checks a password and issues the login cookie
    //
    // We never see or store a plain password: UserManager hashes it before it reaches the
    // database, and CheckPassword compares hashes rather than text.
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // GET /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginDto());
        }

        // POST /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto dto, string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;

            if (!ModelState.IsValid)
                return View(dto);

            // lockoutOnFailure: true means repeated wrong passwords temporarily lock the account.
            // That is what stops someone guessing passwords by brute force.
            var result = await _signInManager.PasswordSignInAsync(
                dto.Email, dto.Password, dto.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
                return SafeRedirect(returnUrl);

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty,
                    "Konto on ajutiselt lukustatud liiga paljude ebaõnnestunud katsete tõttu. Proovi hiljem uuesti.");
                return View(dto);
            }

            // Deliberately vague: saying "no such user" would let an attacker discover which email
            // addresses have accounts. One message covers both wrong email and wrong password.
            ModelState.AddModelError(string.Empty, "Vale e-post või parool.");
            return View(dto);
        }

        // GET /Account/Register
        [HttpGet]
        public IActionResult Register() => View(new RegisterDto());

        // POST /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var user = new ApplicationUser
            {
                UserName = dto.Email,   // Identity uses UserName to log in; we use the email for both
                Email = dto.Email,
                FullName = dto.FullName,
                CreatedAt = DateTime.Now
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
            {
                // Identity's own rules (password too weak, email already taken, ...) come back here.
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                return View(dto);
            }

            // Everyone who registers through this page is a Customer. Admin accounts are created
            // by seeding (see IdentitySeeder), never by self-registration — otherwise anyone
            // could grant themselves the ability to delete the whole catalogue.
            await _userManager.AddToRoleAsync(user, UserRoles.Customer);

            await _signInManager.SignInAsync(user, isPersistent: false);

            TempData["Success"] = "Konto loodud. Tere tulemast!";
            return RedirectToAction("Index", "Home");
        }

        // POST /Account/Logout
        //
        // Logout is a POST, not a link, on purpose: a GET request can be triggered by an image tag
        // or a prefetch on someone else's page, which would let another site sign our users out.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            TempData["Success"] = "Oled välja logitud.";
            return RedirectToAction("Index", "Home");
        }

        // GET /Account/AccessDenied — shown when a signed-in user opens an admin-only page.
        [HttpGet]
        public IActionResult AccessDenied() => View();

        // Same protection as the cart's redirect helper: only ever send the browser to a page on
        // this site, never to an address supplied in the request.
        private IActionResult SafeRedirect(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }
    }
}
