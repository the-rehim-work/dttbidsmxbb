using dttbidsmxbb.Models.DTOs;
using dttbidsmxbb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace dttbidsmxbb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class LogsController(ILogService logService) : Controller
    {
        public IActionResult AuditLogs() => View();
        public IActionResult AuthLogs() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoadAuditLogs()
        {
            var request = DataTableRequest.Parse(Request.Form);
            var result = await logService.GetAuditLogsAsync(request);
            return Json(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoadAuthLogs()
        {
            var request = DataTableRequest.Parse(Request.Form);
            var result = await logService.GetAuthLogsAsync(request);
            return Json(result);
        }
    }
}