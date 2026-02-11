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
                    EF.Functions.Like(x.SendAwaySerialNumber != null ? x.SendAwaySerialNumber.ToLower() : "", $"%{search}%") ||
                    EF.Functions.Like(x.FormalizationSerialNumber != null ? x.FormalizationSerialNumber.ToLower() : "", $"%{search}%") ||
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
                0 => request.SortDirection == "asc" ? query.OrderBy(x => x.SenderMilitaryBase!.Name) : query.OrderByDescending(x => x.SenderMilitaryBase!.Name),
                1 => request.SortDirection == "asc" ? query.OrderBy(x => x.MilitaryBase!.Name) : query.OrderByDescending(x => x.MilitaryBase!.Name),
                2 => request.SortDirection == "asc" ? query.OrderBy(x => x.SentSerialNumber) : query.OrderByDescending(x => x.SentSerialNumber),
                3 => request.SortDirection == "asc" ? query.OrderBy(x => x.SentDate) : query.OrderByDescending(x => x.SentDate),
                4 => request.SortDirection == "asc" ? query.OrderBy(x => x.ReceivedSerialNumber) : query.OrderByDescending(x => x.ReceivedSerialNumber),
                5 => request.SortDirection == "asc" ? query.OrderBy(x => x.ReceivedDate) : query.OrderByDescending(x => x.ReceivedDate),
                6 => request.SortDirection == "asc" ? query.OrderBy(x => x.MilitaryRank!.Name) : query.OrderByDescending(x => x.MilitaryRank!.Name),
                7 => request.SortDirection == "asc" ? query.OrderBy(x => x.RegardingPosition) : query.OrderByDescending(x => x.RegardingPosition),
                8 => request.SortDirection == "asc" ? query.OrderBy(x => x.Position) : query.OrderByDescending(x => x.Position),
                9 => request.SortDirection == "asc" ? query.OrderBy(x => x.Lastname) : query.OrderByDescending(x => x.Lastname),
                10 => request.SortDirection == "asc" ? query.OrderBy(x => x.Firstname) : query.OrderByDescending(x => x.Firstname),
                11 => request.SortDirection == "asc" ? query.OrderBy(x => x.Fathername) : query.OrderByDescending(x => x.Fathername),
                12 => request.SortDirection == "asc" ? query.OrderBy(x => x.AssignmentDate) : query.OrderByDescending(x => x.AssignmentDate),
                13 => request.SortDirection == "asc" ? query.OrderBy(x => x.PrivacyLevel) : query.OrderByDescending(x => x.PrivacyLevel),
                14 => request.SortDirection == "asc" ? query.OrderBy(x => x.SendAwaySerialNumber) : query.OrderByDescending(x => x.SendAwaySerialNumber),
                15 => request.SortDirection == "asc" ? query.OrderBy(x => x.SendAwayDate) : query.OrderByDescending(x => x.SendAwayDate),
                16 => request.SortDirection == "asc" ? query.OrderBy(x => x.Executor!.FullInfo) : query.OrderByDescending(x => x.Executor!.FullInfo),
                17 => request.SortDirection == "asc" ? query.OrderBy(x => x.FormalizationSerialNumber) : query.OrderByDescending(x => x.FormalizationSerialNumber),
                18 => request.SortDirection == "asc" ? query.OrderBy(x => x.FormalizationDate) : query.OrderByDescending(x => x.FormalizationDate),
                19 => request.SortDirection == "asc" ? query.OrderBy(x => x.RejectionInfo) : query.OrderByDescending(x => x.RejectionInfo),
                20 => request.SortDirection == "asc" ? query.OrderBy(x => x.SentBackInfo) : query.OrderByDescending(x => x.SentBackInfo),
                21 => request.SortDirection == "asc" ? query.OrderBy(x => x.Note) : query.OrderByDescending(x => x.Note),
                _ => query.OrderByDescending(x => x.ReceivedDate)
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

        public async Task<List<Information>> GetFilteredListAsync(InformationFilter? filter = null)
        {
            var query = db.Informations
                .Where(x => !x.DeletedAt.HasValue)
                .Include(x => x.MilitaryBase)
                .Include(x => x.SenderMilitaryBase)
                .Include(x => x.MilitaryRank)
                .Include(x => x.Executor)
                .AsNoTracking();

            if (filter is { HasAnyFilter: true })
                query = ApplyFilters(query, filter);

            return await query.OrderByDescending(x => x.ReceivedDate).ToListAsync();
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

            if (!string.IsNullOrEmpty(f.FormalizationSentDateNull))
                query = query.Where(x => f.FormalizationSentDateNull == "null" ? x.SendAwayDate == null : x.SendAwayDate != null);
            else
            {
                if (f.SendAwayDateFrom.HasValue)
                    query = query.Where(x => x.SendAwayDate >= f.SendAwayDateFrom.Value);
                if (f.SendAwayDateTo.HasValue)
                    query = query.Where(x => x.SendAwayDate <= f.SendAwayDateTo.Value);
            }

            if (!string.IsNullOrEmpty(f.FormalizationDateNull))
                query = query.Where(x => f.FormalizationDateNull == "null" ? x.FormalizationDate == null : x.FormalizationDate != null);
            else
            {
                if (f.FormalizationDateFrom.HasValue)
                    query = query.Where(x => x.FormalizationDate >= f.FormalizationDateFrom.Value);
                if (f.FormalizationDateTo.HasValue)
                    query = query.Where(x => x.FormalizationDate <= f.FormalizationDateTo.Value);
            }

            if (!string.IsNullOrEmpty(f.SentSerialNumberQuery))
                query = query.Where(x => x.SentSerialNumber.Contains(f.SentSerialNumberQuery));

            if (!string.IsNullOrEmpty(f.ReceivedSerialNumberQuery))
                query = query.Where(x => x.ReceivedSerialNumber.Contains(f.ReceivedSerialNumberQuery));

            if (!string.IsNullOrEmpty(f.RegardingPositionQuery))
                query = query.Where(x => x.RegardingPosition.Contains(f.RegardingPositionQuery));

            if (!string.IsNullOrEmpty(f.PositionQuery))
                query = query.Where(x => x.Position.Contains(f.PositionQuery));

            if (!string.IsNullOrEmpty(f.FirstnameQuery))
                query = query.Where(x => x.Firstname.Contains(f.FirstnameQuery));

            if (!string.IsNullOrEmpty(f.LastnameNull))
                query = query.Where(x => f.LastnameNull == "null" ? string.IsNullOrEmpty(x.Lastname) : !string.IsNullOrEmpty(x.Lastname));
            else if (!string.IsNullOrEmpty(f.LastnameQuery))
                query = query.Where(x => x.Lastname != null && x.Lastname.Contains(f.LastnameQuery));

            if (!string.IsNullOrEmpty(f.FathernameNull))
                query = query.Where(x => f.FathernameNull == "null" ? string.IsNullOrEmpty(x.Fathername) : !string.IsNullOrEmpty(x.Fathername));
            else if (!string.IsNullOrEmpty(f.FathernameQuery))
                query = query.Where(x => x.Fathername != null && x.Fathername.Contains(f.FathernameQuery));

            if (!string.IsNullOrEmpty(f.FormalizationSentSerialNull))
                query = query.Where(x => f.FormalizationSentSerialNull == "null" ? string.IsNullOrEmpty(x.SendAwaySerialNumber) : !string.IsNullOrEmpty(x.SendAwaySerialNumber));
            else if (!string.IsNullOrEmpty(f.FormalizationSentSerialQuery))
                query = query.Where(x => x.SendAwaySerialNumber != null && x.SendAwaySerialNumber.Contains(f.FormalizationSentSerialQuery));

            if (!string.IsNullOrEmpty(f.FormalizationSerialNull))
                query = query.Where(x => f.FormalizationSerialNull == "null" ? string.IsNullOrEmpty(x.FormalizationSerialNumber) : !string.IsNullOrEmpty(x.FormalizationSerialNumber));
            else if (!string.IsNullOrEmpty(f.FormalizationSerialQuery))
                query = query.Where(x => x.FormalizationSerialNumber != null && x.FormalizationSerialNumber.Contains(f.FormalizationSerialQuery));

            if (!string.IsNullOrEmpty(f.RejectionInfoNull))
                query = query.Where(x => f.RejectionInfoNull == "null" ? string.IsNullOrEmpty(x.RejectionInfo) : !string.IsNullOrEmpty(x.RejectionInfo));
            else if (!string.IsNullOrEmpty(f.RejectionInfoQuery))
                query = query.Where(x => x.RejectionInfo != null && x.RejectionInfo.Contains(f.RejectionInfoQuery));

            if (!string.IsNullOrEmpty(f.SentBackInfoNull))
                query = query.Where(x => f.SentBackInfoNull == "null" ? string.IsNullOrEmpty(x.SentBackInfo) : !string.IsNullOrEmpty(x.SentBackInfo));
            else if (!string.IsNullOrEmpty(f.SentBackInfoQuery))
                query = query.Where(x => x.SentBackInfo != null && x.SentBackInfo.Contains(f.SentBackInfoQuery));

            if (!string.IsNullOrEmpty(f.NoteNull))
                query = query.Where(x => f.NoteNull == "null" ? string.IsNullOrEmpty(x.Note) : !string.IsNullOrEmpty(x.Note));
            else if (!string.IsNullOrEmpty(f.NoteQuery))
                query = query.Where(x => x.Note != null && x.Note.Contains(f.NoteQuery));

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

        public async Task<object> GetDashboardDataAsync(DateOnly? from = null, DateOnly? to = null)
        {
            var query = db.Informations
                .Where(x => !x.DeletedAt.HasValue)
                .AsNoTracking();

            if (from.HasValue)
                query = query.Where(x => x.ReceivedDate >= from.Value);
            if (to.HasValue)
                query = query.Where(x => x.ReceivedDate <= to.Value);

            var totalCount = await query.CountAsync();
            var topSecretCount = await query.CountAsync(x => x.PrivacyLevel == Models.Enum.PrivacyLevel.TopSecret);
            var secretCount = await query.CountAsync(x => x.PrivacyLevel == Models.Enum.PrivacyLevel.Secret);

            var now = DateTime.UtcNow;
            var thisMonthStart = new DateOnly(now.Year, now.Month, 1);
            var thisMonthCount = await query.CountAsync(x => x.ReceivedDate >= thisMonthStart);

            var byBase = await query
                .GroupBy(x => x.MilitaryBase!.Name)
                .Select(g => new { Label = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(15)
                .ToListAsync();

            var byRank = await query
                .GroupBy(x => x.MilitaryRank!.Name)
                .Select(g => new { Label = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(15)
                .ToListAsync();

            var byExecutor = await query
                .GroupBy(x => x.Executor!.FullInfo)
                .Select(g => new { Label = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync();

            var monthlyTrend = await query
                .GroupBy(x => new { x.ReceivedDate.Year, x.ReceivedDate.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync();

            var sentToDtx = await query.CountAsync(x => !string.IsNullOrEmpty(x.SendAwaySerialNumber) || x.SendAwayDate.HasValue);
            var formalized = await query.CountAsync(x => !string.IsNullOrEmpty(x.FormalizationSerialNumber) || x.FormalizationDate.HasValue);
            var rejected = await query.CountAsync(x => !string.IsNullOrEmpty(x.RejectionInfo));
            var sentBack = await query.CountAsync(x => !string.IsNullOrEmpty(x.SentBackInfo));

            return new
            {
                totalCount,
                topSecretCount,
                secretCount,
                thisMonthCount,
                byBase = byBase.Select(x => new { label = x.Label, count = x.Count }),
                byRank = byRank.Select(x => new { label = x.Label, count = x.Count }),
                byExecutor = byExecutor.Select(x => new { label = x.Label, count = x.Count }),
                monthlyTrend = monthlyTrend.Select(x => new { label = $"{x.Year}-{x.Month:D2}", count = x.Count }),
                statusCounts = new { total = totalCount, sentToDtx, formalized, rejected, sentBack }
            };
        }

        public async Task<object> GetBaseBreakdownAsync(int baseId, DateOnly? from = null, DateOnly? to = null)
        {
            var query = db.Informations
                .Where(x => !x.DeletedAt.HasValue && x.MilitaryBaseId == baseId)
                .AsNoTracking();

            if (from.HasValue)
                query = query.Where(x => x.ReceivedDate >= from.Value);
            if (to.HasValue)
                query = query.Where(x => x.ReceivedDate <= to.Value);

            var breakdown = await query
                .GroupBy(x => x.MilitaryRank!.Name)
                .Select(g => new { Label = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            var trend = await query
                .GroupBy(x => new { x.ReceivedDate.Year, x.ReceivedDate.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync();

            return new
            {
                breakdown = breakdown.Select(x => new { label = x.Label, count = x.Count }),
                trend = trend.Select(x => new { label = $"{x.Year}-{x.Month:D2}", count = x.Count })
            };
        }

        public async Task<object> GetRankBreakdownAsync(int rankId, DateOnly? from = null, DateOnly? to = null)
        {
            var query = db.Informations
                .Where(x => !x.DeletedAt.HasValue && x.MilitaryRankId == rankId)
                .AsNoTracking();

            if (from.HasValue)
                query = query.Where(x => x.ReceivedDate >= from.Value);
            if (to.HasValue)
                query = query.Where(x => x.ReceivedDate <= to.Value);

            var breakdown = await query
                .GroupBy(x => x.MilitaryBase!.Name)
                .Select(g => new { Label = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(15)
                .ToListAsync();

            var trend = await query
                .GroupBy(x => new { x.ReceivedDate.Year, x.ReceivedDate.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync();

            return new
            {
                breakdown = breakdown.Select(x => new { label = x.Label, count = x.Count }),
                trend = trend.Select(x => new { label = $"{x.Year}-{x.Month:D2}", count = x.Count })
            };
        }
    }
}