using dttbidsmxbb.Models;
using dttbidsmxbb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace dttbidsmxbb.Controllers
{
    [Authorize]
    public class ImportController(
    IImportService importService,
    ILogService logService,
    UserManager<AppUser> userManager) : Controller
    {
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile file, bool useAsDb = false)
        {
            if (file == null || file.Length == 0)
                return Json(new { success = false, message = "Fayl seçilməyib." });

            var ext = Path.GetExtension(file.FileName).ToLower();
            if (ext != ".xlsx" && ext != ".csv")
                return Json(new { success = false, message = "Yalnız .xlsx və .csv faylları dəstəklənir." });

            using var stream = file.OpenReadStream();
            var result = await importService.ImportAsync(stream, file.FileName, useAsDb);

            await LogImportAsync(
                useAsDb ? "İdxal (DB əvəz)" : "İdxal",
                file.FileName,
                result);

            return Json(new
            {
                result.Success,
                result.TotalRows,
                result.ImportedRows,
                result.SkippedRows,
                result.Errors,
                message = result.Success
                    ? $"{result.ImportedRows} sətir uğurla idxal edildi."
                    : $"{result.ImportedRows} sətir idxal edildi, {result.SkippedRows} sətir keçildi."
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Backup(IFormFile file, string mode = "append")
        {
            if (file == null || file.Length == 0)
                return Json(new { success = false, message = "Fayl seçilməyib." });

            var ext = Path.GetExtension(file.FileName).ToLower();
            if (ext != ".dttbi")
                return Json(new { success = false, message = "Yalnız .dttbi faylları dəstəklənir." });

            var cleanMode = mode == "clean";

            using var stream = file.OpenReadStream();
            var result = await importService.ImportBackupAsync(stream, cleanMode);

            await LogImportAsync(
                cleanMode ? "Ehtiyat bərpa (təmiz)" : "Ehtiyat bərpa (əlavə)",
                file.FileName,
                result);

            var msg = result.ImportedRows > 0
                ? $"{result.ImportedRows} məlumat əlavə edildi."
                : "Heç bir məlumat əlavə edilmədi.";

            if (result.SkippedRows > 0)
                msg += $" {result.SkippedRows} dublikat keçildi.";

            return Json(new
            {
                result.Success,
                result.TotalRows,
                result.ImportedRows,
                result.SkippedRows,
                result.Errors,
                message = msg
            });
        }

        private async Task LogImportAsync(string action, string fileName, Models.DTOs.ImportResult result)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null) return;
            await logService.LogAuditAsync(
                user.Id,
                user.FullName ?? user.UserName!,
                action,
                "Information",
                0,
                null,
                $"{{\"fileName\":\"{fileName}\",\"total\":{result.TotalRows},\"imported\":{result.ImportedRows},\"skipped\":{result.SkippedRows},\"success\":{result.Success.ToString().ToLower()}}}");
        }
    }
}