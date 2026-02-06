namespace dttbidsmxbb.Models.DTOs
{
    public class ImportResult
    {
        public bool Success { get; set; }
        public int TotalRows { get; set; }
        public int ImportedRows { get; set; }
        public int SkippedRows { get; set; }
        public List<ImportError> Errors { get; set; } = [];
    }

    public class ImportError
    {
        public int Row { get; set; }
        public string Field { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}