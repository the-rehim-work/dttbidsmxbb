using dttbidsmxbb.Data;
using dttbidsmxbb.Models;
using dttbidsmxbb.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace dttbidsmxbb.Services
{
    public class InformationService(AppDbContext db) : IInformationService
    {
        public async Task<DataTableResponse<Information>> GetAllAsync(DataTableRequest request, bool includeDeleted = false, InformationFilter? filter = null)
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

            if (filter is { HasAnyFilter: true })
                query = ApplyFilters(query, filter);

            if (!string.IsNullOrEmpty(request.SearchValue))
            {
                var search = request.SearchValue.ToLower();
                query = query.Where(x =>
                    EF.Functions.Like(x.Firstname.ToLower(), $"%{search}%") ||
                    (x.Lastname != null && EF.Functions.Like(x.Lastname.ToLower(), $"%{search}%")) ||
                    (x.Fathername != null && EF.Functions.Like(x.Fathername.ToLower(), $"%{search}%")) ||
                    EF.Functions.Like(x.SentSerialNumber.ToLower(), $"%{search}%") ||
                    EF.Functions.Like(x.ReceivedSerialNumber.ToLower(), $"%{search}%") ||
                    EF.Functions.Like(x.SendAwaySerialNumber.ToLower(), $"%{search}%") ||
                    EF.Functions.Like(x.FormalizationSerialNumber.ToLower(), $"%{search}%") ||
                    EF.Functions.Like(x.Position.ToLower(), $"%{search}%") ||
                    EF.Functions.Like(x.RegardingPosition.ToLower(), $"%{search}%") ||
                    (x.MilitaryBase != null && EF.Functions.Like(x.MilitaryBase.Name.ToLower(), $"%{search}%")) ||
                    (x.SenderMilitaryBase != null && EF.Functions.Like(x.SenderMilitaryBase.Name.ToLower(), $"%{search}%")) ||
                    (x.MilitaryRank != null && EF.Functions.Like(x.MilitaryRank.Name.ToLower(), $"%{search}%")) ||
                    (x.Executor != null && EF.Functions.Like(x.Executor.FullInfo.ToLower(), $"%{search}%")) ||
                    (x.Note != null && EF.Functions.Like(x.Note.ToLower(), $"%{search}%")));
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

        private static IQueryable<Information> ApplyFilters(IQueryable<Information> query, InformationFilter f)
        {
            if (f.MilitaryBaseIds.Count > 0)
                query = query.Where(x => f.MilitaryBaseIds.Contains(x.MilitaryBaseId));

            if (f.SenderMilitaryBaseIds.Count > 0)
                query = query.Where(x => f.SenderMilitaryBaseIds.Contains(x.SenderMilitaryBaseId));

            if (f.MilitaryRankIds.Count > 0)
                query = query.Where(x => f.MilitaryRankIds.Contains(x.MilitaryRankId));

            if (f.ExecutorIds.Count > 0)
                query = query.Where(x => f.ExecutorIds.Contains(x.ExecutorId));

            if (f.PrivacyLevels.Count > 0)
            {
                var levels = f.PrivacyLevels.Cast<Models.Enum.PrivacyLevel>().ToList();
                query = query.Where(x => levels.Contains(x.PrivacyLevel));
            }

            if (f.SentDateFrom.HasValue)
                query = query.Where(x => x.SentDate >= f.SentDateFrom.Value);
            if (f.SentDateTo.HasValue)
                query = query.Where(x => x.SentDate <= f.SentDateTo.Value);

            if (f.ReceivedDateFrom.HasValue)
                query = query.Where(x => x.ReceivedDate >= f.ReceivedDateFrom.Value);
            if (f.ReceivedDateTo.HasValue)
                query = query.Where(x => x.ReceivedDate <= f.ReceivedDateTo.Value);

            if (f.AssignmentDateFrom.HasValue)
                query = query.Where(x => x.AssignmentDate >= f.AssignmentDateFrom.Value);
            if (f.AssignmentDateTo.HasValue)
                query = query.Where(x => x.AssignmentDate <= f.AssignmentDateTo.Value);

            if (f.SendAwayDateFrom.HasValue)
                query = query.Where(x => x.SendAwayDate >= f.SendAwayDateFrom.Value);
            if (f.SendAwayDateTo.HasValue)
                query = query.Where(x => x.SendAwayDate <= f.SendAwayDateTo.Value);

            if (f.FormalizationDateFrom.HasValue)
                query = query.Where(x => x.FormalizationDate >= f.FormalizationDateFrom.Value);
            if (f.FormalizationDateTo.HasValue)
                query = query.Where(x => x.FormalizationDate <= f.FormalizationDateTo.Value);

            if (f.RejectionInfoNull == "null")
                query = query.Where(x => x.RejectionInfo == null || x.RejectionInfo == "");
            else if (f.RejectionInfoNull == "notnull")
                query = query.Where(x => x.RejectionInfo != null && x.RejectionInfo != "");

            if (f.SentBackInfoNull == "null")
                query = query.Where(x => x.SentBackInfo == null || x.SentBackInfo == "");
            else if (f.SentBackInfoNull == "notnull")
                query = query.Where(x => x.SentBackInfo != null && x.SentBackInfo != "");

            if (f.NoteNull == "null")
                query = query.Where(x => x.Note == null || x.Note == "");
            else if (f.NoteNull == "notnull")
                query = query.Where(x => x.Note != null && x.Note != "");

            if (f.LastnameNull == "null")
                query = query.Where(x => x.Lastname == null || x.Lastname == "");
            else if (f.LastnameNull == "notnull")
                query = query.Where(x => x.Lastname != null && x.Lastname != "");

            if (f.FathernameNull == "null")
                query = query.Where(x => x.Fathername == null || x.Fathername == "");
            else if (f.FathernameNull == "notnull")
                query = query.Where(x => x.Fathername != null && x.Fathername != "");

            return query;
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