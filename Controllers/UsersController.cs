using dttbidsmxbb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace dttbidsmxbb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController(
        UserManager<AppUser> userManager,
        RoleManager<AppRole> roleManager) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var users = await userManager.Users.ToListAsync();
            var userList = new List<object>();

            foreach (var user in users)
            {
                var roles = await userManager.GetRolesAsync(user);
                userList.Add(new
                {
                    user.Id,
                    user.UserName,
                    user.FullName,
                    Roles = string.Join(", ", roles)
                });
            }

            ViewBag.Users = userList;
            ViewBag.Roles = new SelectList(
                await roleManager.Roles.ToListAsync(), "Name", "Name");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string userName, string fullName, string password, string role)
        {
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
                return Json(new { success = false, message = "İstifadəçi adı və şifrə mütləqdir." });

            var existing = await userManager.FindByNameAsync(userName);
            if (existing != null)
                return Json(new { success = false, message = "Bu istifadəçi adı artıq mövcuddur." });

            var user = new AppUser
            {
                UserName = userName.Trim(),
                FullName = fullName?.Trim()
            };

            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                var errors = string.Join(" ", result.Errors.Select(e => e.Description));
                return Json(new { success = false, message = $"Xəta: {errors}" });
            }

            if (!string.IsNullOrWhiteSpace(role))
                await userManager.AddToRoleAsync(user, role);

            return Json(new { success = true, message = "İstifadəçi uğurla yaradıldı." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, string fullName, string? role)
        {
            var user = await userManager.FindByIdAsync(id.ToString());
            if (user == null)
                return Json(new { success = false, message = "İstifadəçi tapılmadı." });

            user.FullName = fullName?.Trim();
            await userManager.UpdateAsync(user);

            if (!string.IsNullOrWhiteSpace(role))
            {
                var currentRoles = await userManager.GetRolesAsync(user);
                await userManager.RemoveFromRolesAsync(user, currentRoles);
                await userManager.AddToRoleAsync(user, role);
            }

            return Json(new { success = true, message = "İstifadəçi uğurla yeniləndi." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(int id, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
                return Json(new { success = false, message = "Yeni şifrə mütləqdir." });

            var user = await userManager.FindByIdAsync(id.ToString());
            if (user == null)
                return Json(new { success = false, message = "İstifadəçi tapılmadı." });

            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var result = await userManager.ResetPasswordAsync(user, token, newPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(" ", result.Errors.Select(e => e.Description));
                return Json(new { success = false, message = $"Xəta: {errors}" });
            }

            return Json(new { success = true, message = "Şifrə uğurla sıfırlandı." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await userManager.FindByIdAsync(id.ToString());
            if (user == null)
                return Json(new { success = false, message = "İstifadəçi tapılmadı." });

            if (user.UserName?.ToLower() == "admin")
                return Json(new { success = false, message = "Admin istifadəçisi silinə bilməz." });

            var currentUser = await userManager.GetUserAsync(User);
            if (currentUser?.Id == id)
                return Json(new { success = false, message = "Özünüzü silə bilməzsiniz." });

            await userManager.DeleteAsync(user);
            return Json(new { success = true, message = "İstifadəçi uğurla silindi." });
        }
    }
}