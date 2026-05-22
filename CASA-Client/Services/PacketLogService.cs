using CASA_Client.Models;

namespace CASA_Client.Services;

public class PacketLogService
{
    private readonly List<PacketLog> _logs = new();

    public IReadOnlyList<PacketLog> Logs => _logs;

    public event Action? OnChanged;

    public void Add(string protocol, string action, string message, double? elapsedMs = null)
    {
        _logs.Add(new PacketLog(protocol, action, message, elapsedMs));
        OnChanged?.Invoke();
    }

    public void Clear()
    {
        _logs.Clear();
        OnChanged?.Invoke();
    }
}
