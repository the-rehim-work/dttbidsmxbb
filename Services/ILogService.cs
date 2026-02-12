using dttbidsmxbb.Models;
using dttbidsmxbb.Models.DTOs;

namespace dttbidsmxbb.Services
{
    public interface ILogService
    {
        Task LogAuditAsync(int userId, string userFullName, string action, string entityName, int entityId, string? oldValues, string? newValues);
        Task LogAuthAsync(string username, bool success, string ipAddress, string? failureReason = null);
        Task<DataTableResponse<AuditLog>> GetAuditLogsAsync(DataTableRequest request);
        Task<DataTableResponse<AuthLog>> GetAuthLogsAsync(DataTableRequest request);
    }
}