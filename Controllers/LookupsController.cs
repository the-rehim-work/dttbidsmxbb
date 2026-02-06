using dttbidsmxbb.Data;
using dttbidsmxbb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dttbidsmxbb.Controllers
{
    [Authorize]
    public class LookupsController(AppDbContext db) : Controller
    {
        public async Task<IActionResult> MilitaryRanks()
        {
            var data = await db.MilitaryRanks.OrderBy(x => x.Name).ToListAsync();
            return View(data);
        }

        public async Task<IActionResult> MilitaryBases()
        {
            var data = await db.MilitaryBases.OrderBy(x => x.Name).ToListAsync();
            return View(data);
        }

        public async Task<IActionResult> Executors()
        {
            var data = await db.Executors.OrderBy(x => x.FullInfo).ToListAsync();
            return View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMilitaryRank(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Json(new { success = false, message = "Ad sahəsi mütləqdir." });

            if (await db.MilitaryRanks.AnyAsync(x => x.Name == name.Trim()))
                return Json(new { success = false, message = "Bu rütbə artıq mövcuddur." });

            db.MilitaryRanks.Add(new MilitaryRank { Name = name.Trim() });
            await db.SaveChangesAsync();
            return Json(new { success = true, message = "Rütbə uğurla əlavə edildi." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateMilitaryRank(int id, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Json(new { success = false, message = "Ad sahəsi mütləqdir." });

            var entity = await db.MilitaryRanks.FindAsync(id);
            if (entity == null)
                return Json(new { success = false, message = "Rütbə tapılmadı." });

            if (await db.MilitaryRanks.AnyAsync(x => x.Name == name.Trim() && x.Id != id))
                return Json(new { success = false, message = "Bu rütbə artıq mövcuddur." });

            entity.Name = name.Trim();
            await db.SaveChangesAsync();
            return Json(new { success = true, message = "Rütbə uğurla yeniləndi." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMilitaryRank(int id)
        {
            var entity = await db.MilitaryRanks.FindAsync(id);
            if (entity == null)
                return Json(new { success = false, message = "Rütbə tapılmadı." });

            var inUse = await db.Informations.AnyAsync(x => x.MilitaryRankId == id);
            if (inUse)
                return Json(new { success = false, message = "Bu rütbə istifadə olunur və silinə bilməz." });

            db.MilitaryRanks.Remove(entity);
            await db.SaveChangesAsync();
            return Json(new { success = true, message = "Rütbə uğurla silindi." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMilitaryBase(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Json(new { success = false, message = "Ad sahəsi mütləqdir." });

            if (await db.MilitaryBases.AnyAsync(x => x.Name == name.Trim()))
                return Json(new { success = false, message = "Bu hərbi hissə artıq mövcuddur." });

            db.MilitaryBases.Add(new MilitaryBase { Name = name.Trim() });
            await db.SaveChangesAsync();
            return Json(new { success = true, message = "Hərbi hissə uğurla əlavə edildi." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateMilitaryBase(int id, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Json(new { success = false, message = "Ad sahəsi mütləqdir." });

            var entity = await db.MilitaryBases.FindAsync(id);
            if (entity == null)
                return Json(new { success = false, message = "Hərbi hissə tapılmadı." });

            if (await db.MilitaryBases.AnyAsync(x => x.Name == name.Trim() && x.Id != id))
                return Json(new { success = false, message = "Bu hərbi hissə artıq mövcuddur." });

            entity.Name = name.Trim();
            await db.SaveChangesAsync();
            return Json(new { success = true, message = "Hərbi hissə uğurla yeniləndi." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMilitaryBase(int id)
        {
            var entity = await db.MilitaryBases.FindAsync(id);
            if (entity == null)
                return Json(new { success = false, message = "Hərbi hissə tapılmadı." });

            var inUse = await db.Informations.AnyAsync(x => x.MilitaryBaseId == id || x.SenderMilitaryBaseId == id);
            if (inUse)
                return Json(new { success = false, message = "Bu hərbi hissə istifadə olunur və silinə bilməz." });

            db.MilitaryBases.Remove(entity);
            await db.SaveChangesAsync();
            return Json(new { success = true, message = "Hərbi hissə uğurla silindi." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateExecutor(string fullInfo)
        {
            if (string.IsNullOrWhiteSpace(fullInfo))
                return Json(new { success = false, message = "Məlumat sahəsi mütləqdir." });

            if (await db.Executors.AnyAsync(x => x.FullInfo == fullInfo.Trim()))
                return Json(new { success = false, message = "Bu icraçı artıq mövcuddur." });

            db.Executors.Add(new Executor { FullInfo = fullInfo.Trim() });
            await db.SaveChangesAsync();
            return Json(new { success = true, message = "İcraçı uğurla əlavə edildi." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateExecutor(int id, string fullInfo)
        {
            if (string.IsNullOrWhiteSpace(fullInfo))
                return Json(new { success = false, message = "Məlumat sahəsi mütləqdir." });

            var entity = await db.Executors.FindAsync(id);
            if (entity == null)
                return Json(new { success = false, message = "İcraçı tapılmadı." });

            if (await db.Executors.AnyAsync(x => x.FullInfo == fullInfo.Trim() && x.Id != id))
                return Json(new { success = false, message = "Bu icraçı artıq mövcuddur." });

            entity.FullInfo = fullInfo.Trim();
            await db.SaveChangesAsync();
            return Json(new { success = true, message = "İcraçı uğurla yeniləndi." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteExecutor(int id)
        {
            var entity = await db.Executors.FindAsync(id);
            if (entity == null)
                return Json(new { success = false, message = "İcraçı tapılmadı." });

            var inUse = await db.Informations.AnyAsync(x => x.ExecutorId == id);
            if (inUse)
                return Json(new { success = false, message = "Bu icraçı istifadə olunur və silinə bilməz." });

            db.Executors.Remove(entity);
            await db.SaveChangesAsync();
            return Json(new { success = true, message = "İcraçı uğurla silindi." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuickAdd(string type, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return Json(new { success = false, message = "Dəyər mütləqdir." });

            var trimmed = value.Trim();
            int id;

            switch (type)
            {
                case "rank":
                    if (await db.MilitaryRanks.AnyAsync(x => x.Name == trimmed))
                        return Json(new { success = false, message = "Artıq mövcuddur." });
                    var rank = new MilitaryRank { Name = trimmed };
                    db.MilitaryRanks.Add(rank);
                    await db.SaveChangesAsync();
                    id = rank.Id;
                    break;

                case "base":
                    if (await db.MilitaryBases.AnyAsync(x => x.Name == trimmed))
                        return Json(new { success = false, message = "Artıq mövcuddur." });
                    var mb = new MilitaryBase { Name = trimmed };
                    db.MilitaryBases.Add(mb);
                    await db.SaveChangesAsync();
                    id = mb.Id;
                    break;

                case "executor":
                    if (await db.Executors.AnyAsync(x => x.FullInfo == trimmed))
                        return Json(new { success = false, message = "Artıq mövcuddur." });
                    var exec = new Executor { FullInfo = trimmed };
                    db.Executors.Add(exec);
                    await db.SaveChangesAsync();
                    id = exec.Id;
                    break;

                default:
                    return Json(new { success = false, message = "Yanlış tip." });
            }

            return Json(new { success = true, id, name = trimmed, message = "Uğurla əlavə edildi." });
        }
    }
}