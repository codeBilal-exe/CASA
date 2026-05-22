namespace CASA_Client.Models;

public class PacketLog
{
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public string Protocol { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public double? ElapsedMs { get; set; }

    // Convenience ctor
    public PacketLog(string protocol, string action, string message, double? elapsedMs = null)
    {
        Protocol = protocol;
        Action = action;
        Message = message;
        ElapsedMs = elapsedMs;
    }
}
