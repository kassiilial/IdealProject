namespace Config;

public class CacheSettings
{
    public int DefaultTtlSeconds { get; set; } = 300;
    public bool UseSlidingExpiration { get; set; } = false;
}
