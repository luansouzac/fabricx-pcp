using FabricX.Infrastructure.Cache;
using FabricX.Tests.Fakes;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace FabricX.Tests.Infrastructure;

public class CacheDeConsultasMemoriaTests : IDisposable
{
    private readonly MemoryCache _memoria = new(new MemoryCacheOptions());
    private readonly LoggerColetor<CacheDeConsultasMemoria> _logger = new();

    public void Dispose() => _memoria.Dispose();

    private CacheDeConsultasMemoria CriarCache(TimeSpan? expiracao = null) =>
        new(_memoria, Options.Create(new CacheOptions { Expiracao = expiracao ?? TimeSpan.FromMinutes(1) }), _logger);

    private static Func<Task<int>> Fonte(Counter contador) => () => Task.FromResult(++contador.Valor);

    private sealed class Counter
    {
        public int Valor;
    }

    [Fact]
    public async Task PrimeiraChamadaBuscaNaFonteESegundaVemDoCache()
    {
        var cache = CriarCache();
        var contador = new Counter();

        var primeira = await cache.ObterOuBuscarAsync("k", Fonte(contador));
        var segunda = await cache.ObterOuBuscarAsync("k", Fonte(contador));

        Assert.Equal(1, primeira);
        Assert.Equal(1, segunda);
        Assert.Equal(1, contador.Valor);
    }

    [Fact]
    public async Task ChavesDiferentesNaoSeMisturam()
    {
        var cache = CriarCache();
        var contador = new Counter();

        var a = await cache.ObterOuBuscarAsync("a", Fonte(contador));
        var b = await cache.ObterOuBuscarAsync("b", Fonte(contador));

        Assert.Equal(1, a);
        Assert.Equal(2, b);
    }

    [Fact]
    public async Task InvalidarTudo_ForcaNovaBuscaNaFonte()
    {
        var cache = CriarCache();
        var contador = new Counter();
        await cache.ObterOuBuscarAsync("a", Fonte(contador));
        await cache.ObterOuBuscarAsync("b", Fonte(contador));

        cache.InvalidarTudo();

        Assert.Equal(3, await cache.ObterOuBuscarAsync("a", Fonte(contador)));
        Assert.Equal(4, await cache.ObterOuBuscarAsync("b", Fonte(contador)));
    }

    [Fact]
    public async Task Expiracao_DepoisDoTempoBuscaNaFonteDeNovo()
    {
        var cache = CriarCache(TimeSpan.FromMilliseconds(100));
        var contador = new Counter();
        await cache.ObterOuBuscarAsync("k", Fonte(contador));

        await Task.Delay(300);

        Assert.Equal(2, await cache.ObterOuBuscarAsync("k", Fonte(contador)));
    }

    [Fact]
    public async Task InvalidacaoDuranteABusca_NaoDeixaValorAntigoNoCache()
    {
        var cache = CriarCache();
        var contador = new Counter();

        await cache.ObterOuBuscarAsync("k", async () =>
        {
            cache.InvalidarTudo();
            return await Task.FromResult(++contador.Valor);
        });

        Assert.Equal(2, await cache.ObterOuBuscarAsync("k", Fonte(contador)));
    }

    [Fact]
    public async Task Log_IdentificaMissHitEInvalidacaoEmOrdem()
    {
        var cache = CriarCache();
        var contador = new Counter();

        await cache.ObterOuBuscarAsync("indicadores:resumo", Fonte(contador));
        await cache.ObterOuBuscarAsync("indicadores:resumo", Fonte(contador));
        cache.InvalidarTudo();

        Assert.Collection(_logger.Mensagens,
            m => Assert.StartsWith("CACHE MISS indicadores:resumo", m),
            m => Assert.StartsWith("CACHE HIT indicadores:resumo", m),
            m => Assert.StartsWith("CACHE INVALIDADO", m));
    }

    [Fact]
    public async Task ExcecaoNaFonte_NaoGuardaNada()
    {
        var cache = CriarCache();
        var contador = new Counter();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            cache.ObterOuBuscarAsync<int>("k", () => throw new InvalidOperationException("falhou")));

        Assert.Equal(1, await cache.ObterOuBuscarAsync("k", Fonte(contador)));
    }
}
