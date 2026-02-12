using dttbidsmxbb.Models;
using dttbidsmxbb.Models.DTOs;
using dttbidsmxbb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace dttbidsmxbb.Controllers
{
    [Authorize]
    public class ExportController(
    IInformationService informationService,
    IExportService exportService,
    ILogService logService,
    UserManager<AppUser> userManager) : Controller
    {
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pdf()
        {
            var (data, cols) = await GetExportContext();
            var bytes = await exportService.ExportToPdfAsync(data, cols);
            await LogExportAsync("PDF", data.Count);
            return File(bytes, "application/pdf", $"Məlumatlar_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Excel()
        {
            var (data, cols) = await GetExportContext();
            var bytes = await exportService.ExportToExcelAsync(data, cols);
            await LogExportAsync("Excel", data.Count);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Məlumatlar_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Word()
        {
            var (data, cols) = await GetExportContext();
            var bytes = await exportService.ExportToWordAsync(data, cols);
            await LogExportAsync("Word", data.Count);
            return File(bytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                $"Məlumatlar_{DateTime.Now:yyyyMMdd_HHmmss}.docx");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Print()
        {
            var (data, cols) = await GetExportContext();
            var html = exportService.GeneratePrintHtml(data, cols);
            await LogExportAsync("Print", data.Count);
            return Content(html, "text/html");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Backup()
        {
            var filter = InformationFilter.Parse(Request.Form);
            var data = await informationService.GetFilteredListAsync(filter.HasAnyFilter ? filter : null);
            var bytes = await exportService.ExportBackupAsync(data);
            await LogExportAsync("Backup", data.Count);
            return File(bytes, "application/octet-stream", $"Ehtiyat_{DateTime.Now:yyyyMMdd_HHmmss}.dttbi");
        }

        [HttpGet]
        public async Task<IActionResult> Template()
        {
            var bytes = await exportService.GenerateImportTemplateAsync();
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "İdxal_Şablonu.xlsx");
        }

        private async Task LogExportAsync(string format, int recordCount)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null) return;
            await logService.LogAuditAsync(
                user.Id,
                user.FullName ?? user.UserName!,
                $"İxrac ({format})",
                "Information",
                0,
                null,
                $"{{\"format\":\"{format}\",\"recordCount\":{recordCount}}}");
        }

        private async Task<(List<Models.Information>, int[]?)> GetExportContext()
        {
            var filter = InformationFilter.Parse(Request.Form);
            var data = await informationService.GetFilteredListAsync(filter.HasAnyFilter ? filter : null);
            var colsRaw = Request.Form["visibleColumns"].ToString();
            int[]? cols = null;
            if (!string.IsNullOrWhiteSpace(colsRaw))
            {
                cols = colsRaw.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => int.TryParse(x.Trim(), out var v) ? v : -1)
                    .Where(x => x >= 0)
                    .ToArray();
                if (cols.Length == 0) cols = null;
            }
            return (data, cols);
        }
    }
}