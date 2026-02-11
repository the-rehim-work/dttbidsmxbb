namespace dttbidsmxbb.Models.DTOs
{
    public class BackupExportDto
    {
        public int Version { get; set; } = 1;
        public DateTime ExportedAt { get; set; }
        public int RecordCount { get; set; }
        public List<InformationBackupDto> Records { get; set; } = [];
    }
}
