using FabricX.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace FabricX.Infrastructure.Cache;

public sealed class CacheDeConsultasMemoria : ICacheDeConsultas
{
    private readonly IMemoryCache _cache;
    private readonly CacheOptions _opcoes;
    private readonly ILogger<CacheDeConsultasMemoria> _logger;
    private readonly object _trava = new();
    private CancellationTokenSource _invalidador = new();

    public CacheDeConsultasMemoria(IMemoryCache cache, IOptions<CacheOptions> opcoes, ILogger<CacheDeConsultasMemoria> logger)
    {
        _cache = cache;
        _opcoes = opcoes.Value;
        _logger = logger;
    }

    public async Task<T> ObterOuBuscarAsync<T>(string chave, Func<Task<T>> buscar)
    {
        if (_cache.TryGetValue(chave, out T? guardado) && guardado is not null)
        {
            _logger.LogInformation("CACHE HIT {Chave}: valor devolvido do cache", chave);
            return guardado;
        }

        _logger.LogInformation("CACHE MISS {Chave}: buscando na fonte original", chave);
        var tokenDaBusca = ObterTokenAtual();
        var valor = await buscar();

        var entrada = new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = _opcoes.Expiracao };
        entrada.AddExpirationToken(new CancellationChangeToken(tokenDaBusca));
        _cache.Set(chave, valor, entrada);

        return valor;
    }

    public void InvalidarTudo()
    {
        CancellationTokenSource anterior;

        lock (_trava)
        {
            anterior = _invalidador;
            _invalidador = new CancellationTokenSource();
        }

        anterior.Cancel();
        _logger.LogInformation("CACHE INVALIDADO: uma escrita bem-sucedida descartou as consultas em cache");
    }

    private CancellationToken ObterTokenAtual()
    {
        lock (_trava)
            return _invalidador.Token;
    }
}
