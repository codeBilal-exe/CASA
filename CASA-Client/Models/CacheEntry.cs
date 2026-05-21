namespace CASA_Client.Models;

public class CacheEntry
{
    public string Url { get; set; } = string.Empty;
    public string ResolvedIp { get; set; } = string.Empty;
    public int Port { get; set; }
    public string HtmlContent { get; set; } = string.Empty;
    public DateTime CachedAt { get; set; } = DateTime.Now;
    public int HitCount { get; set; } = 0;
    public TimeSpan Ttl { get; set; } = TimeSpan.FromMinutes(5);
    public bool IsExpired => DateTime.Now - CachedAt > Ttl;
    public TimeSpan TtlRemaining => IsExpired ? TimeSpan.Zero : (Ttl - (DateTime.Now - CachedAt));
    public long SizeBytes => System.Text.Encoding.UTF8.GetByteCount(HtmlContent);
}
