using dttbidsmxbb.Models;

namespace dttbidsmxbb.Services
{
    public interface IExportService
    {
        Task<byte[]> ExportToPdfAsync(List<Information> data, int[]? visibleColumns = null);
        Task<byte[]> ExportToExcelAsync(List<Information> data, int[]? visibleColumns = null);
        Task<byte[]> ExportToWordAsync(List<Information> data, int[]? visibleColumns = null);
        Task<byte[]> GenerateImportTemplateAsync();
        Task<byte[]> ExportBackupAsync(List<Information> data);
        string GeneratePrintHtml(List<Information> data, int[]? visibleColumns = null);
    }
}