namespace dttbidsmxbb.Models.DTOs
{
    public class InformationBackupDto
    {
        public string MilitaryBaseName { get; set; } = string.Empty;
        public string SenderMilitaryBaseName { get; set; } = string.Empty;
        public string SentSerialNumber { get; set; } = string.Empty;
        public DateOnly SentDate { get; set; }
        public string ReceivedSerialNumber { get; set; } = string.Empty;
        public DateOnly ReceivedDate { get; set; }
        public string MilitaryRankName { get; set; } = string.Empty;
        public string RegardingPosition { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string? Lastname { get; set; }
        public string Firstname { get; set; } = string.Empty;
        public string? Fathername { get; set; }
        public DateOnly AssignmentDate { get; set; }
        public int PrivacyLevel { get; set; }
        public string? SendAwaySerialNumber { get; set; }
        public DateOnly? SendAwayDate { get; set; }
        public string ExecutorFullInfo { get; set; } = string.Empty;
        public string? FormalizationSerialNumber { get; set; }
        public DateOnly? FormalizationDate { get; set; }
        public string? RejectionInfo { get; set; }
        public string? SentBackInfo { get; set; }
        public string? Note { get; set; }
    }
}
