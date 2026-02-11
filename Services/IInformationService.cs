using dttbidsmxbb.Models;
using dttbidsmxbb.Models.DTOs;

namespace dttbidsmxbb.Services
{
    public interface IInformationService
    {
        Task<DataTableResponse<Information>> GetAllAsync(DataTableRequest request, bool includeDeleted = false, InformationFilter? filter = null);
        Task<Information?> GetByIdAsync(int id);
        Task<Information> CreateAsync(Information entity);
        Task<bool> UpdateAsync(Information entity);
        Task<bool> SoftDeleteAsync(int id);
        Task<bool> RestoreAsync(int id);
        Task<object> GetDashboardDataAsync(DateOnly? from = null, DateOnly? to = null);
        Task<object> GetBaseBreakdownAsync(int baseId, DateOnly? from = null, DateOnly? to = null);
        Task<object> GetRankBreakdownAsync(int rankId, DateOnly? from = null, DateOnly? to = null);
    }
}