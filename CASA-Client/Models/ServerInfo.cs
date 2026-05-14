namespace CASA_Client.Models;

public class ServerInfo
{
    public string Name { get; set; } = string.Empty;
    public string ExePath { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Protocol { get; set; } = string.Empty;
    public int Port { get; set; }
    public bool IsOnline { get; set; } = false;
}
