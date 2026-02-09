using dttbidsmxbb.Data;
using dttbidsmxbb.Models;
using dttbidsmxbb.Models.DTOs;
using dttbidsmxbb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace dttbidsmxbb.Controllers
{
    [Authorize]
    public class InformationsController(
        AppDbContext db,
        IInformationService informationService,
        ILogService logService,
        UserManager<AppUser> userManager) : Controller
    {
        public async Task<IActionResult> Index()
        {
            await PopulateViewBagsAsync();
            return View(new Information());
        }

        [HttpPost]
        public async Task<IActionResult> Load()
        {
            var request = DataTableRequest.Parse(Request.Form);
            var showDeleted = Request.Form["showDeleted"].ToString() == "true";
            var filter = InformationFilter.Parse(Request.Form);
            var result = await informationService.GetAllAsync(request, showDeleted, filter);
            return Json(result);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateViewBagsAsync();
            return View("Upsert", new Information());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Information model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateViewBagsAsync();
                return View("Upsert", model);
            }

            var entity = await informationService.CreateAsync(model);
            var user = await userManager.GetUserAsync(User);
            await logService.LogAuditAsync(
                user!.Id,
                user.FullName ?? user.UserName!,
                "Yaradıldı",
                "Information",
                entity.Id,
                null,
                JsonSerializer.Serialize(entity, JsonOpts));

            TempData["ToastType"] = "success";
            TempData["ToastMessage"] = "Məlumat uğurla yaradıldı.";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var entity = await informationService.GetByIdAsync(id);
            if (entity == null)
                return NotFound();

            await PopulateViewBagsAsync();
            return View("Upsert", entity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Information model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateViewBagsAsync();
                return View("Upsert", model);
            }

            var old = await informationService.GetByIdAsync(model.Id);
            if (old == null)
                return NotFound();

            var oldJson = JsonSerializer.Serialize(old, JsonOpts);
            var success = await informationService.UpdateAsync(model);
            if (!success)
                return NotFound();

            var updated = await informationService.GetByIdAsync(model.Id);
            var user = await userManager.GetUserAsync(User);
            await logService.LogAuditAsync(
                user!.Id,
                user.FullName ?? user.UserName!,
                "Redaktə edildi",
                "Information",
                model.Id,
                oldJson,
                JsonSerializer.Serialize(updated, JsonOpts));

            TempData["ToastType"] = "success";
            TempData["ToastMessage"] = "Məlumat uğurla yeniləndi.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await informationService.GetByIdAsync(id);
            if (entity == null)
                return Json(new { success = false, message = "Məlumat tapılmadı." });

            var oldJson = JsonSerializer.Serialize(entity, JsonOpts);
            var success = await informationService.SoftDeleteAsync(id);
            if (!success)
                return Json(new { success = false, message = "Silinmə zamanı xəta baş verdi." });

            var user = await userManager.GetUserAsync(User);
            await logService.LogAuditAsync(
                user!.Id,
                user.FullName ?? user.UserName!,
                "Silindi",
                "Information",
                id,
                oldJson,
                null);

            return Json(new { success = true, message = "Məlumat uğurla silindi." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            var success = await informationService.RestoreAsync(id);
            if (!success)
                return Json(new { success = false, message = "Bərpa zamanı xəta baş verdi." });

            var entity = await informationService.GetByIdAsync(id);
            var user = await userManager.GetUserAsync(User);
            await logService.LogAuditAsync(
                user!.Id,
                user.FullName ?? user.UserName!,
                "Bərpa edildi",
                "Information",
                id,
                null,
                JsonSerializer.Serialize(entity, JsonOpts));

            return Json(new { success = true, message = "Məlumat uğurla bərpa edildi." });
        }

        private async Task PopulateViewBagsAsync()
        {
            ViewBag.MilitaryBases = new SelectList(
                await db.MilitaryBases.OrderBy(x => x.Name).ToListAsync(), "Id", "Name");
            ViewBag.MilitaryRanks = new SelectList(
                await db.MilitaryRanks.OrderBy(x => x.Name).ToListAsync(), "Id", "Name");
            ViewBag.Executors = new SelectList(
                await db.Executors.OrderBy(x => x.FullInfo).ToListAsync(), "Id", "FullInfo");
        }

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            WriteIndented = false,
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
        };
    }
}