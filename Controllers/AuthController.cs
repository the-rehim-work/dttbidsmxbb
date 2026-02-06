using dttbidsmxbb.Models;
using dttbidsmxbb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace dttbidsmxbb.Controllers
{
    public class AuthController(
        SignInManager<AppUser> signInManager,
        UserManager<AppUser> userManager,
        ILogService logService) : Controller
    {
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password, string? returnUrl = null)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                await logService.LogAuthAsync(username ?? "", false, ip, "Boş istifadəçi adı və ya şifrə");
                ViewBag.Error = "İstifadəçi adı və şifrə daxil edin.";
                return View();
            }

            var user = await userManager.FindByNameAsync(username);
            if (user == null)
            {
                await logService.LogAuthAsync(username, false, ip, "İstifadəçi tapılmadı");
                ViewBag.Error = "İstifadəçi adı və ya şifrə yanlışdır.";
                return View();
            }

            var result = await signInManager.PasswordSignInAsync(user, password, isPersistent: false, lockoutOnFailure: false);
            if (!result.Succeeded)
            {
                await logService.LogAuthAsync(username, false, ip, "Yanlış şifrə");
                ViewBag.Error = "İstifadəçi adı və ya şifrə yanlışdır.";
                return View();
            }

            await logService.LogAuthAsync(username, true, ip);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var username = User.Identity?.Name ?? "";
            await signInManager.SignOutAsync();
            await logService.LogAuthAsync(username, true, ip);
            return RedirectToAction("Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeMyPassword(string currentPassword, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
                return Json(new { success = false, message = "Yeni şifrələr uyğun gəlmir." });

            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return Json(new { success = false, message = "İstifadəçi tapılmadı." });

            var result = await userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(" ", result.Errors.Select(e => e.Description));
                return Json(new { success = false, message = $"Şifrə dəyişdirilə bilmədi: {errors}" });
            }

            await signInManager.RefreshSignInAsync(user);
            return Json(new { success = true, message = "Şifrə uğurla dəyişdirildi." });
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}