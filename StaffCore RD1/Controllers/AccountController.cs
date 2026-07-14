using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StaffCore_RD1.Models;
using System.Diagnostics;
using System.Security.Claims;

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
                    // 1. Guardamos el nombre en los Claims del usuario
                    await _userManager.AddClaimAsync(user, new Claim("NombreCompleto", model.NombreCompleto));

                    // 2. Lógica de roles (que ya tenías)
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

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> MiPerfil()
        {
            var user = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(user);

            // Enviamos los roles a la vista usando ViewBag
            ViewBag.Roles = roles;
            return View(user);
        }
    }
}