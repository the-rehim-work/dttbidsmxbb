using dttbidsmxbb.Models;
using dttbidsmxbb.Models.DTOs;

namespace dttbidsmxbb.Services
{
    public interface IInformationService
    {
        Task<DataTableResponse<Information>> GetAllAsync(DataTableRequest request, bool includeDeleted = false);
        Task<Information?> GetByIdAsync(int id);
        Task<Information> CreateAsync(Information entity);
        Task<bool> UpdateAsync(Information entity);
        Task<bool> SoftDeleteAsync(int id);
        Task<bool> RestoreAsync(int id);
        Task<object> GetDashboardDataAsync();
    }
}