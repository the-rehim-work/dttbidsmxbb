using dttbidsmxbb.Data;
using dttbidsmxbb.Models;
using dttbidsmxbb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Globalization;

namespace dttbidsmxbb.Controllers
{
    [Authorize]
    public class HomeController(
        IInformationService informationService,
        AppDbContext db) : Controller
    {
        public async Task<IActionResult> Index()
        {
            ViewBag.MilitaryBases = new SelectList(
                await db.MilitaryBases.OrderBy(x => x.Name).ToListAsync(), "Id", "Name");
            ViewBag.MilitaryRanks = new SelectList(
                await db.MilitaryRanks.OrderBy(x => x.Name).ToListAsync(), "Id", "Name");
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboardData(string? from, string? to)
        {
            var data = await informationService.GetDashboardDataAsync(ParseDate(from), ParseDate(to));
            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetBaseBreakdown(int id, string? from, string? to)
        {
            var data = await informationService.GetBaseBreakdownAsync(id, ParseDate(from), ParseDate(to));
            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetRankBreakdown(int id, string? from, string? to)
        {
            var data = await informationService.GetRankBreakdownAsync(id, ParseDate(from), ParseDate(to));
            return Json(data);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private static readonly string[] DateFormats = ["yyyy-MM-dd", "dd.MM.yyyy"];

        private static DateOnly? ParseDate(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            if (DateOnly.TryParseExact(raw.Trim(), DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
                return d;
            return null;
        }
    }
}