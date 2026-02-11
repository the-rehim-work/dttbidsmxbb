using dttbidsmxbb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace dttbidsmxbb.Controllers
{
    [Authorize]
    public class ImportController(IImportService importService) : Controller
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
    }
}