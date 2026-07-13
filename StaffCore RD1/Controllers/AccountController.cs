using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StaffCore_RD1.Models;
using System.Diagnostics;

namespace StaffCore_RD1.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new IdentityUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Asigna Administrador si es el primer usuario, de lo contrario Viewer
                    var isFirstUser = _userManager.Users.Count() == 1;
                    string assignedRole = isFirstUser ? "Administrador" : "Viewer";

                    await _userManager.AddToRoleAsync(user, assignedRole);
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    TempData["Exito"] = $"¡Registro exitoso! Se te asignó el rol: {assignedRole}";
                    return RedirectToAction("Index", "Staff");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // lockoutOnFailure habilitado en true
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Staff");
                }
                if (result.IsLockedOut)
                {
                    ModelState.AddModelError(string.Empty, "La cuenta está bloqueada temporalmente debido a múltiples intentos fallidos.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Credenciales incorrectas. Verifique e intente nuevamente.");
                }
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
    }
}