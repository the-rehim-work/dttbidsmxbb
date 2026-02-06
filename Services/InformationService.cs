using dttbidsmxbb.Data;
using dttbidsmxbb.Models;
using dttbidsmxbb.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace dttbidsmxbb.Services
{
    public class InformationService(AppDbContext db) : IInformationService
    {
        public async Task<DataTableResponse<Information>> GetAllAsync(DataTableRequest request, bool includeDeleted = false)
        {
            var query = db.Informations
                .Include(x => x.MilitaryBase)
                .Include(x => x.SenderMilitaryBase)
                .Include(x => x.MilitaryRank)
                .Include(x => x.Executor)
                .AsNoTracking();

            if (!includeDeleted)
                query = query.Where(x => !x.DeletedAt.HasValue);

            var totalCount = await query.CountAsync();

            if (!string.IsNullOrEmpty(request.SearchValue))
            {
                var search = request.SearchValue.ToLower();
                query = query.Where(x =>
                    (x.Firstname != null && x.Firstname.Contains(search, StringComparison.CurrentCultureIgnoreCase)) ||
                    (x.Lastname != null && x.Lastname.Contains(search, StringComparison.CurrentCultureIgnoreCase)) ||
                    (x.Fathername != null && x.Fathername.Contains(search, StringComparison.CurrentCultureIgnoreCase)) ||
                    x.SentSerialNumber.Contains(search, StringComparison.CurrentCultureIgnoreCase) ||
                    x.ReceivedSerialNumber.Contains(search, StringComparison.CurrentCultureIgnoreCase) ||
                    x.SendAwaySerialNumber.Contains(search, StringComparison.CurrentCultureIgnoreCase) ||
                    x.FormalizationSerialNumber.Contains(search, StringComparison.CurrentCultureIgnoreCase) ||
                    x.Position.Contains(search, StringComparison.CurrentCultureIgnoreCase) ||
                    x.RegardingPosition.Contains(search, StringComparison.CurrentCultureIgnoreCase) ||
                    (x.MilitaryBase != null && x.MilitaryBase.Name.Contains(search, StringComparison.CurrentCultureIgnoreCase)) ||
                    (x.SenderMilitaryBase != null && x.SenderMilitaryBase.Name.Contains(search, StringComparison.CurrentCultureIgnoreCase)) ||
                    (x.MilitaryRank != null && x.MilitaryRank.Name.Contains(search, StringComparison.CurrentCultureIgnoreCase)) ||
                    (x.Executor != null && x.Executor.FullInfo.Contains(search, StringComparison.CurrentCultureIgnoreCase)) ||
                    (x.Note != null && x.Note.Contains(search, StringComparison.CurrentCultureIgnoreCase)));
            }

            var filteredCount = await query.CountAsync();

            query = request.SortColumnIndex switch
            {
                0 => request.SortDirection == "asc" ? query.OrderBy(x => x.Id) : query.OrderByDescending(x => x.Id),
                1 => request.SortDirection == "asc" ? query.OrderBy(x => x.SenderMilitaryBase!.Name) : query.OrderByDescending(x => x.SenderMilitaryBase!.Name),
                2 => request.SortDirection == "asc" ? query.OrderBy(x => x.MilitaryBase!.Name) : query.OrderByDescending(x => x.MilitaryBase!.Name),
                3 => request.SortDirection == "asc" ? query.OrderBy(x => x.SentSerialNumber) : query.OrderByDescending(x => x.SentSerialNumber),
                4 => request.SortDirection == "asc" ? query.OrderBy(x => x.SentDate) : query.OrderByDescending(x => x.SentDate),
                5 => request.SortDirection == "asc" ? query.OrderBy(x => x.ReceivedSerialNumber) : query.OrderByDescending(x => x.ReceivedSerialNumber),
                6 => request.SortDirection == "asc" ? query.OrderBy(x => x.ReceivedDate) : query.OrderByDescending(x => x.ReceivedDate),
                7 => request.SortDirection == "asc" ? query.OrderBy(x => x.MilitaryRank!.Name) : query.OrderByDescending(x => x.MilitaryRank!.Name),
                8 => request.SortDirection == "asc" ? query.OrderBy(x => x.RegardingPosition) : query.OrderByDescending(x => x.RegardingPosition),
                9 => request.SortDirection == "asc" ? query.OrderBy(x => x.Position) : query.OrderByDescending(x => x.Position),
                10 => request.SortDirection == "asc" ? query.OrderBy(x => x.Lastname) : query.OrderByDescending(x => x.Lastname),
                11 => request.SortDirection == "asc" ? query.OrderBy(x => x.Firstname) : query.OrderByDescending(x => x.Firstname),
                12 => request.SortDirection == "asc" ? query.OrderBy(x => x.Fathername) : query.OrderByDescending(x => x.Fathername),
                13 => request.SortDirection == "asc" ? query.OrderBy(x => x.AssignmentDate) : query.OrderByDescending(x => x.AssignmentDate),
                14 => request.SortDirection == "asc" ? query.OrderBy(x => x.PrivacyLevel) : query.OrderByDescending(x => x.PrivacyLevel),
                15 => request.SortDirection == "asc" ? query.OrderBy(x => x.SendAwaySerialNumber) : query.OrderByDescending(x => x.SendAwaySerialNumber),
                16 => request.SortDirection == "asc" ? query.OrderBy(x => x.SendAwayDate) : query.OrderByDescending(x => x.SendAwayDate),
                17 => request.SortDirection == "asc" ? query.OrderBy(x => x.Executor!.FullInfo) : query.OrderByDescending(x => x.Executor!.FullInfo),
                18 => request.SortDirection == "asc" ? query.OrderBy(x => x.FormalizationSerialNumber) : query.OrderByDescending(x => x.FormalizationSerialNumber),
                19 => request.SortDirection == "asc" ? query.OrderBy(x => x.FormalizationDate) : query.OrderByDescending(x => x.FormalizationDate),
                _ => query.OrderByDescending(x => x.Id)
            };

            var data = await query.Skip(request.Start).Take(request.Length).ToListAsync();

            return new DataTableResponse<Information>
            {
                Draw = request.Draw,
                RecordsTotal = totalCount,
                RecordsFiltered = filteredCount,
                Data = data
            };
        }

        public async Task<Information?> GetByIdAsync(int id)
        {
            return await db.Informations
                .Include(x => x.MilitaryBase)
                .Include(x => x.SenderMilitaryBase)
                .Include(x => x.MilitaryRank)
                .Include(x => x.Executor)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Information> CreateAsync(Information entity)
        {
            entity.CreatedAt = DateTime.UtcNow;
            db.Informations.Add(entity);
            await db.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> UpdateAsync(Information entity)
        {
            var existing = await db.Informations.FindAsync(entity.Id);
            if (existing?.DeletedAt.HasValue != false)
                return false;

            existing.MilitaryBaseId = entity.MilitaryBaseId;
            existing.SenderMilitaryBaseId = entity.SenderMilitaryBaseId;
            existing.SentSerialNumber = entity.SentSerialNumber;
            existing.SentDate = entity.SentDate;
            existing.ReceivedSerialNumber = entity.ReceivedSerialNumber;
            existing.ReceivedDate = entity.ReceivedDate;
            existing.MilitaryRankId = entity.MilitaryRankId;
            existing.RegardingPosition = entity.RegardingPosition;
            existing.Position = entity.Position;
            existing.Lastname = entity.Lastname;
            existing.Firstname = entity.Firstname;
            existing.Fathername = entity.Fathername;
            existing.AssignmentDate = entity.AssignmentDate;
            existing.PrivacyLevel = entity.PrivacyLevel;
            existing.SendAwaySerialNumber = entity.SendAwaySerialNumber;
            existing.SendAwayDate = entity.SendAwayDate;
            existing.ExecutorId = entity.ExecutorId;
            existing.FormalizationSerialNumber = entity.FormalizationSerialNumber;
            existing.FormalizationDate = entity.FormalizationDate;
            existing.RejectionInfo = entity.RejectionInfo;
            existing.SentBackInfo = entity.SentBackInfo;
            existing.Note = entity.Note;
            existing.ModifiedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SoftDeleteAsync(int id)
        {
            var entity = await db.Informations.FindAsync(id);
            if (entity?.DeletedAt.HasValue != false)
                return false;

            entity.DeletedAt = DateTime.UtcNow;
            entity.ModifiedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RestoreAsync(int id)
        {
            var entity = await db.Informations.FindAsync(id);
            if (entity?.DeletedAt.HasValue != true)
                return false;

            entity.DeletedAt = null;
            entity.ModifiedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return true;
        }

        public async Task<object> GetDashboardDataAsync()
        {
            var byBase = await db.Informations
                .Where(x => !x.DeletedAt.HasValue)
                .GroupBy(x => x.MilitaryBase!.Name)
                .Select(g => new { Label = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(15)
                .ToListAsync();

            var monthlyTrend = await db.Informations
                .Where(x => !x.DeletedAt.HasValue)
                .GroupBy(x => new { x.CreatedAt.Year, x.CreatedAt.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Count = g.Count()
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync();

            return new
            {
                byBase,
                monthlyTrend = monthlyTrend.Select(x => new
                {
                    label = $"{x.Year}-{x.Month:D2}",
                    count = x.Count
                })
            };
        }
    }
}