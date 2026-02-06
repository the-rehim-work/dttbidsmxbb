namespace dttbidsmxbb.Models
{
    public class AuthLog
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string? FailureReason { get; set; }
        public DateTime Timestamp { get; set; }
    }
}