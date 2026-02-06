namespace dttbidsmxbb.Models
{
    public class EventLog
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string? UserFullName { get; set; }
        public string Method { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}