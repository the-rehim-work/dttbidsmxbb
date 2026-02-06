using dttbidsmxbb.Models;

namespace dttbidsmxbb.Services
{
    public interface IExportService
    {
        Task<byte[]> ExportToPdfAsync(List<Information> data);
        Task<byte[]> ExportToExcelAsync(List<Information> data);
        Task<byte[]> ExportToWordAsync(List<Information> data);
        Task<byte[]> GenerateImportTemplateAsync();
    }
}