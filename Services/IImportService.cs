using dttbidsmxbb.Models.DTOs;

namespace dttbidsmxbb.Services
{
    public interface IImportService
    {
        Task<ImportResult> ImportAsync(Stream fileStream, string fileName, bool useAsDb);
    }
}