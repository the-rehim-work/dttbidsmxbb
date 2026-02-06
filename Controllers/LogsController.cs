using dttbidsmxbb.Models.DTOs;
using dttbidsmxbb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace dttbidsmxbb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class LogsController(ILogService logService) : Controller
    {
        public IActionResult AuditLogs()
        {
            return View();
        }

        public IActionResult AuthLogs()
        {
            return View();
        }

        public IActionResult EventLogs()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LoadAuditLogs()
        {
            var request = DataTableRequest.Parse(Request.Form);
            var result = await logService.GetAuditLogsAsync(request);
            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> LoadAuthLogs()
        {
            var request = DataTableRequest.Parse(Request.Form);
            var result = await logService.GetAuthLogsAsync(request);
            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> LoadEventLogs()
        {
            var request = DataTableRequest.Parse(Request.Form);
            var result = await logService.GetEventLogsAsync(request);
            return Json(result);
        }
    }
}