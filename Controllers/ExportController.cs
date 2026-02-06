using dttbidsmxbb.Data;
using dttbidsmxbb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dttbidsmxbb.Controllers
{
    [Authorize]
    public class ExportController(AppDbContext db, IExportService exportService) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Pdf()
        {
            var data = await GetExportData();
            var bytes = await exportService.ExportToPdfAsync(data);
            return File(bytes, "application/pdf", $"Məlumatlar_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
        }

        [HttpGet]
        public async Task<IActionResult> Excel()
        {
            var data = await GetExportData();
            var bytes = await exportService.ExportToExcelAsync(data);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Məlumatlar_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        }

        [HttpGet]
        public async Task<IActionResult> Word()
        {
            var data = await GetExportData();
            var bytes = await exportService.ExportToWordAsync(data);
            return File(bytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                $"Məlumatlar_{DateTime.Now:yyyyMMdd_HHmmss}.docx");
        }

        [HttpGet]
        public async Task<IActionResult> Template()
        {
            var bytes = await exportService.GenerateImportTemplateAsync();
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "İdxal_Şablonu.xlsx");
        }

        private async Task<List<Models.Information>> GetExportData()
        {
            return await db.Informations
                .Where(x => !x.DeletedAt.HasValue)
                .Include(x => x.MilitaryBase)
                .Include(x => x.SenderMilitaryBase)
                .Include(x => x.MilitaryRank)
                .Include(x => x.Executor)
                .OrderByDescending(x => x.Id)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}