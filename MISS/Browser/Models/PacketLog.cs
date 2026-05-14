using System;

namespace MISS.Browser.Models
{
    public class PacketLog
    {
        public DateTime Timestamp { get; set; }
        public string Protocol { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"[{Timestamp:HH:mm:ss.fff}] [{Protocol}] {Action}: {Message}";
        }
    }
}
