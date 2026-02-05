namespace dttbidsmxbb.Models
{
    public class Information
    {
        public int Id { get; set; }
        public int MilitaryBaseId { get; set; }
        public virtual MilitaryBase? MilitaryBase { get; set; }
        public int SenderMilitaryBaseId { get; set; }
        public virtual MilitaryBase? SenderMilitaryBase { get; set; }
        public string SentSerialNumber { get; set; } = string.Empty;
        public DateOnly SentDate { get; set; }
        public string ReceivedSerialNumber { get; set; } = string.Empty;
        public DateOnly ReceivedDate { get; set; }
        public int MilitaryRankId { get; set; }
        public virtual MilitaryRank? MilitaryRank { get; set; }
        public string RegardingPosition { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string? Lastname { get; set; }
        public string Firstname { get; set; } = string.Empty;
        public string? Fathername { get; set; }
        public DateOnly AssignmentDate { get; set; }
        public int PrivacyLevel { get; set; }
        public string SendAwaySerialNumber { get; set; } = string.Empty;
        public DateOnly SendAwayDate { get; set; }
        public int ExecutorId { get; set; }
        public virtual Executor? Executor { get; set; }
        public string FormalizationSerialNumber { get; set; } = string.Empty;
        public DateOnly FormalizationDate { get; set; }
        public string? RejectionInfo { get; set; }
        public string? SentBackInfo { get; set; }
        public string? Note { get; set; }
    }
}
