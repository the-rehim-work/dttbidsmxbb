using dttbidsmxbb.Data;
using dttbidsmxbb.Models;
using dttbidsmxbb.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace dttbidsmxbb.Services
{
    public class LogService(AppDbContext db) : ILogService
    {
        public async Task LogAuditAsync(int userId, string userFullName, string action, string entityName, int entityId, string? oldValues, string? newValues)
        {
            db.AuditLogs.Add(new AuditLog
            {
                UserId = userId,
                UserFullName = userFullName,
                Action = action,
                EntityName = entityName,
                EntityId = entityId,
                OldValues = oldValues,
                NewValues = newValues,
                Timestamp = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        public async Task LogAuthAsync(string username, bool success, string ipAddress, string? failureReason = null)
        {
            db.AuthLogs.Add(new AuthLog
            {
                Username = username,
                Success = success,
                IpAddress = ipAddress,
                FailureReason = failureReason,
                Timestamp = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        public async Task LogEventAsync(int? userId, string? userFullName, string method, string path, int statusCode, string ipAddress)
        {
            db.EventLogs.Add(new EventLog
            {
                UserId = userId,
                UserFullName = userFullName,
                Method = method,
                Path = path,
                StatusCode = statusCode,
                IpAddress = ipAddress,
                Timestamp = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        public async Task<DataTableResponse<AuditLog>> GetAuditLogsAsync(DataTableRequest request)
        {
            var query = db.AuditLogs.AsNoTracking();
            var totalCount = await query.CountAsync();

            if (!string.IsNullOrEmpty(request.SearchValue))
            {
                var search = request.SearchValue.ToLower();
                query = query.Where(x =>
                    x.UserFullName.ToLower().Contains(search) ||
                    x.Action.ToLower().Contains(search) ||
                    x.EntityName.ToLower().Contains(search));
            }

            var filteredCount = await query.CountAsync();

            query = request.SortColumnIndex switch
            {
                0 => request.SortDirection == "asc" ? query.OrderBy(x => x.Id) : query.OrderByDescending(x => x.Id),
                1 => request.SortDirection == "asc" ? query.OrderBy(x => x.UserFullName) : query.OrderByDescending(x => x.UserFullName),
                2 => request.SortDirection == "asc" ? query.OrderBy(x => x.Action) : query.OrderByDescending(x => x.Action),
                3 => request.SortDirection == "asc" ? query.OrderBy(x => x.EntityName) : query.OrderByDescending(x => x.EntityName),
                4 => request.SortDirection == "asc" ? query.OrderBy(x => x.EntityId) : query.OrderByDescending(x => x.EntityId),
                5 => request.SortDirection == "asc" ? query.OrderBy(x => x.Timestamp) : query.OrderByDescending(x => x.Timestamp),
                _ => query.OrderByDescending(x => x.Timestamp)
            };

            var data = await query.Skip(request.Start).Take(request.Length).ToListAsync();

            return new DataTableResponse<AuditLog>
            {
                Draw = request.Draw,
                RecordsTotal = totalCount,
                RecordsFiltered = filteredCount,
                Data = data
            };
        }

        public async Task<DataTableResponse<AuthLog>> GetAuthLogsAsync(DataTableRequest request)
        {
            var query = db.AuthLogs.AsNoTracking();
            var totalCount = await query.CountAsync();

            if (!string.IsNullOrEmpty(request.SearchValue))
            {
                var search = request.SearchValue.ToLower();
                query = query.Where(x =>
                    x.Username.ToLower().Contains(search) ||
                    x.IpAddress.Contains(search));
            }

            var filteredCount = await query.CountAsync();

            query = request.SortColumnIndex switch
            {
                0 => request.SortDirection == "asc" ? query.OrderBy(x => x.Id) : query.OrderByDescending(x => x.Id),
                1 => request.SortDirection == "asc" ? query.OrderBy(x => x.Username) : query.OrderByDescending(x => x.Username),
                2 => request.SortDirection == "asc" ? query.OrderBy(x => x.Success) : query.OrderByDescending(x => x.Success),
                3 => request.SortDirection == "asc" ? query.OrderBy(x => x.IpAddress) : query.OrderByDescending(x => x.IpAddress),
                4 => request.SortDirection == "asc" ? query.OrderBy(x => x.Timestamp) : query.OrderByDescending(x => x.Timestamp),
                _ => query.OrderByDescending(x => x.Timestamp)
            };

            var data = await query.Skip(request.Start).Take(request.Length).ToListAsync();

            return new DataTableResponse<AuthLog>
            {
                Draw = request.Draw,
                RecordsTotal = totalCount,
                RecordsFiltered = filteredCount,
                Data = data
            };
        }

        public async Task<DataTableResponse<EventLog>> GetEventLogsAsync(DataTableRequest request)
        {
            var query = db.EventLogs.AsNoTracking();
            var totalCount = await query.CountAsync();

            if (!string.IsNullOrEmpty(request.SearchValue))
            {
                var search = request.SearchValue.ToLower();
                query = query.Where(x =>
                    (x.UserFullName != null && x.UserFullName.ToLower().Contains(search)) ||
                    x.Path.ToLower().Contains(search) ||
                    x.Method.ToLower().Contains(search));
            }

            var filteredCount = await query.CountAsync();

            query = request.SortColumnIndex switch
            {
                0 => request.SortDirection == "asc" ? query.OrderBy(x => x.Id) : query.OrderByDescending(x => x.Id),
                1 => request.SortDirection == "asc" ? query.OrderBy(x => x.UserFullName) : query.OrderByDescending(x => x.UserFullName),
                2 => request.SortDirection == "asc" ? query.OrderBy(x => x.Method) : query.OrderByDescending(x => x.Method),
                3 => request.SortDirection == "asc" ? query.OrderBy(x => x.Path) : query.OrderByDescending(x => x.Path),
                4 => request.SortDirection == "asc" ? query.OrderBy(x => x.StatusCode) : query.OrderByDescending(x => x.StatusCode),
                5 => request.SortDirection == "asc" ? query.OrderBy(x => x.Timestamp) : query.OrderByDescending(x => x.Timestamp),
                _ => query.OrderByDescending(x => x.Timestamp)
            };

            var data = await query.Skip(request.Start).Take(request.Length).ToListAsync();

            return new DataTableResponse<EventLog>
            {
                Draw = request.Draw,
                RecordsTotal = totalCount,
                RecordsFiltered = filteredCount,
                Data = data
            };
        }
    }
}