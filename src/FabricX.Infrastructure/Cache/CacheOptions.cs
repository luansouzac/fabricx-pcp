namespace FabricX.Infrastructure.Cache;

public class CacheOptions
{
    public const string Secao = "Cache";

    public TimeSpan Expiracao { get; set; } = TimeSpan.FromSeconds(60);
}
