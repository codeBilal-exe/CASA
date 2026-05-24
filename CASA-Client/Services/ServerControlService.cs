using System.Diagnostics;
using CASA_Client.Models;

namespace CASA_Client.Services;

public class ServerControlService : IDisposable
{
    private readonly Dictionary<string, Process> _processes = new();
    private readonly CacheService _cache;

    // Base path: root of the CASA project (4 levels up from Browser/bin/Debug/net8.0)
    private readonly string _basePath =
        Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../.."));

    public List<ServerInfo> Servers { get; } = new()
    {
        new() { Name = "DNS Resolver",   ExePath = "Servers/DNS_Server/DNS_Server.exe",   Description = "Domain → IP resolver",     Protocol = "UDP", Port = 5053 },
        new() { Name = "Load Balancer",  ExePath = "Servers/Load_Balancer/Load_Balancer.exe", Description = "Distributes traffic",  Protocol = "TCP", Port = 8080 },
        new() { Name = "Apple Website 1",ExePath = "Websites/Apple/apple_Servers/apple_server.exe",     Description = "Think Different",          Protocol = "TCP", Port = 8081 },
        new() { Name = "Apple Website 2",ExePath = "Websites/Apple/apple_Servers/apple2_server.exe",   Description = "Think Different 2",        Protocol = "TCP", Port = 8084 },
        new() { Name = "Apple Website 3",ExePath = "Websites/Apple/apple_Servers/apple3_server.exe",   Description = "Think Different 3",        Protocol = "TCP", Port = 8085 },
        new() { Name = "Google Website 1",ExePath = "Websites/Google/google-Servers/google_server.exe",  Description = "Search engine simulation", Protocol = "TCP", Port = 8082 },
        new() { Name = "Google Website 2",ExePath = "Websites/Google/google-Servers/google2_server.exe",Description = "Search engine simulation 2", Protocol = "TCP", Port = 8086 },
        new() { Name = "GitHub Website", ExePath = "Websites/Github/github_server.exe",   Description = "Repository simulation",    Protocol = "TCP", Port = 8083 },
        new() { Name = "YouTube Website",ExePath = "Websites/Youtube/yoututbe-Servers/youtube_server.exe",Description = "Video streaming simulation", Protocol = "TCP", Port = 8087 },
    };

    public event Action? OnChanged;

    public ServerControlService(CacheService cache)
    {
        _cache = cache;
    }

    public void Toggle(ServerInfo server)
    {
        if (server.IsOnline) Stop(server);
        else Start(server);
    }

    public void Start(ServerInfo server)
    {
        if (IsRunning(server.Name)) return;

        string fullPath = Path.Combine(_basePath, server.ExePath);
        string? workDir = Path.GetDirectoryName(fullPath);
        if (workDir is null || !File.Exists(fullPath)) return;

        var psi = new ProcessStartInfo
        {
            FileName = fullPath,
            WorkingDirectory = workDir,
            UseShellExecute = true,
            CreateNoWindow = false,
            WindowStyle = ProcessWindowStyle.Minimized
        };

        var proc = Process.Start(psi);
        if (proc is not null)
        {
            _processes[server.Name] = proc;
            server.IsOnline = true;
            OnChanged?.Invoke();
        }
    }

    public void Stop(ServerInfo server)
    {
        if (_processes.TryGetValue(server.Name, out var proc))
        {
            try { if (!proc.HasExited) proc.Kill(); } catch { }
            _processes.Remove(server.Name);
        }
        server.IsOnline = false;
        _cache.ClearByPort(server.Port);
        OnChanged?.Invoke();
    }

    public bool IsRunning(string name)
    {
        if (_processes.TryGetValue(name, out var proc))
        {
            if (proc.HasExited) { _processes.Remove(name); return false; }
            return true;
        }
        return false;
    }

    public void StopAll()
    {
        foreach (var s in Servers) Stop(s);
    }

    public void Dispose() => StopAll();
}
