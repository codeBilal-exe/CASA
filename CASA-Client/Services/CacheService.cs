using System.Collections.Concurrent;
using CASA_Client.Models;

namespace CASA_Client.Services;

public class CacheService
{
    private readonly ConcurrentDictionary<string, CacheEntry> _store = new();
    public event Action? OnChanged;

    // Settings
    public bool IsEnabled { get; private set; } = true;
    public int DefaultTtlMinutes { get; private set; } = 5;
    public bool AutoClearOnServerStop { get; private set; } = true;

    public bool TryGet(string url, out CacheEntry? entry)
    {
        if (!IsEnabled || !_store.TryGetValue(url.ToLower(), out entry))
        {
            entry = null;
            return false;
        }
        if (entry.IsExpired)
        {
            _store.TryRemove(url.ToLower(), out _);
            entry = null;
            return false;
        }
        entry.HitCount++;
        OnChanged?.Invoke();
        return true;
    }

    public void Set(string url, string ip, int port, string htmlContent)
    {
        if (!IsEnabled) return;
        var entry = new CacheEntry
        {
            Url = url.ToLower(),
            ResolvedIp = ip,
            Port = port,
            HtmlContent = htmlContent,
            CachedAt = DateTime.Now,
            HitCount = 0,
            Ttl = TimeSpan.FromMinutes(DefaultTtlMinutes)
        };
        _store[url.ToLower()] = entry;
        OnChanged?.Invoke();
    }

    public void Remove(string url)
    {
        _store.TryRemove(url.ToLower(), out _);
        OnChanged?.Invoke();
    }

    public void Clear()
    {
        _store.Clear();
        OnChanged?.Invoke();
    }

    public List<CacheEntry> GetAll() =>
        _store.Values.OrderByDescending(e => e.CachedAt).ToList();

    public int TotalHits => _store.Values.Sum(e => e.HitCount);
    public long TotalSizeBytes => _store.Values.Sum(e => e.SizeBytes);
    public int Count => _store.Count;

    public void SetEnabled(bool enabled) { IsEnabled = enabled; OnChanged?.Invoke(); }
    public void SetTtl(int minutes) { DefaultTtlMinutes = Math.Max(1, minutes); }
    public void SetAutoClear(bool value) { AutoClearOnServerStop = value; }

    // Called when a server stops — removes entries cached from that server's port
    public void ClearByPort(int port)
    {
        if (!AutoClearOnServerStop) return;
        var toRemove = _store.Where(kvp => kvp.Value.Port == port).Select(kvp => kvp.Key).ToList();
        foreach (var key in toRemove) _store.TryRemove(key, out _);
        if (toRemove.Count > 0) OnChanged?.Invoke();
    }
}
