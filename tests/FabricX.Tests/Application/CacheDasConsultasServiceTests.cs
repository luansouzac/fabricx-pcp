using FabricX.Application.Interfaces;
using FabricX.Application.Services;
using FabricX.Domain.Entities;
using FabricX.Domain.Enums;
using FabricX.Infrastructure.Cache;
using FabricX.Tests.Fakes;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace FabricX.Tests.Application;

public class CacheDasConsultasServiceTests : IDisposable
{
    private static readonly CancellationToken Ct = CancellationToken.None;

    private readonly MemoryCache _memoria = new(new MemoryCacheOptions());
    private readonly LoggerColetor<CacheDeConsultasMemoria> _logger = new();
    private readonly CacheDeConsultasMemoria _cache;

    private readonly RecursoRepositorySpy _recursos = new();
    private readonly ApontamentoRepositorySpy _apontamentos;
    private readonly ProgramacaoRepositoryFake _programacoes = new();
    private readonly OrdemProducaoRepositoryFake _ordens = new();
    private readonly ProdutoRepositoryFake _produtos = new();
    private readonly RoteiroEtapaRepositoryFake _etapas = new();
    private readonly DemandaPrevistaRepositoryFake _demandas = new();

    public CacheDasConsultasServiceTests()
    {
        _cache = new CacheDeConsultasMemoria(_memoria, Options.Create(new CacheOptions()), _logger);
        _apontamentos = new ApontamentoRepositorySpy(new ApontamentoRepositoryFake(_programacoes));
    }

    public void Dispose() => _memoria.Dispose();

    private IndicadorService Indicadores() => new(_apontamentos, _programacoes, _ordens, _etapas, _produtos, _cache);

    private CapacidadeService Capacidade() => new(_recursos, _programacoes, _demandas, _etapas, _cache);

    [Fact]
    public async Task Indicadores_SegundaChamadaIgualNaoVaiAoRepositorio()
    {
        var servico = Indicadores();

        await servico.CalcularAsync(null, null, null, null, Ct);
        await servico.CalcularAsync(null, null, null, null, Ct);

        Assert.Equal(1, _apontamentos.Consultas);
    }

    [Fact]
    public async Task Indicadores_FiltrosDiferentesGeramEntradasDiferentes()
    {
        var servico = Indicadores();

        await servico.CalcularAsync(null, null, null, null, Ct);
        await servico.CalcularAsync(null, null, 1, null, Ct);
        await servico.CalcularAsync(new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30), null, null, Ct);

        Assert.Equal(3, _apontamentos.Consultas);
    }

    [Fact]
    public async Task Indicadores_DepoisDeInvalidarVoltaAConsultarARepositorio()
    {
        var servico = Indicadores();
        await servico.CalcularAsync(null, null, null, null, Ct);

        _cache.InvalidarTudo();
        await servico.CalcularAsync(null, null, null, null, Ct);

        Assert.Equal(2, _apontamentos.Consultas);
    }

    [Fact]
    public async Task Resumo_SegundaChamadaVemDoCacheMesmoComOEstoqueAlteradoPorFora()
    {
        var servico = Indicadores();
        var primeiro = await servico.ObterResumoAsync(Ct);

        await _produtos.AdicionarAsync(ProdutoComEstoqueBaixo(), Ct);
        var segundo = await servico.ObterResumoAsync(Ct);

        Assert.Empty(primeiro.ProdutosComEstoqueBaixo);
        Assert.Empty(segundo.ProdutosComEstoqueBaixo);

        _cache.InvalidarTudo();
        var depois = await servico.ObterResumoAsync(Ct);
        Assert.Single(depois.ProdutosComEstoqueBaixo);
    }

    [Fact]
    public async Task Capacidade_SegundaChamadaDoMesmoPeriodoNaoVaiAoRepositorio()
    {
        var servico = Capacidade();

        await servico.CalcularAsync("2026-12", Ct);
        await servico.CalcularAsync("2026-12", Ct);
        await servico.CalcularAsync("2027-01", Ct);

        Assert.Equal(2, _recursos.Consultas);
    }

    [Fact]
    public async Task Capacidade_PeriodoInvalidoContinuaDando400AntesDeTocarNoCache()
    {
        await Assert.ThrowsAnyAsync<FabricX.Domain.Exceptions.DomainException>(() => Capacidade().CalcularAsync("2026-13", Ct));

        Assert.Empty(_logger.Mensagens);
    }

    private static Produto ProdutoComEstoqueBaixo() =>
        new("MP-X", "Material", TipoProduto.MateriaPrima, estoqueAtual: 5, estoqueMinimo: 10);

    private sealed class ApontamentoRepositorySpy(ApontamentoRepositoryFake interno) : IApontamentoRepository
    {
        public int Consultas { get; private set; }

        public Task<IReadOnlyList<Apontamento>> ListarPorProgramacaoAsync(int programacaoId, CancellationToken cancellationToken) =>
            interno.ListarPorProgramacaoAsync(programacaoId, cancellationToken);

        public Task<IReadOnlyList<Apontamento>> ListarPorOrdemAsync(int ordemId, CancellationToken cancellationToken) =>
            interno.ListarPorOrdemAsync(ordemId, cancellationToken);

        public Task<IReadOnlyList<Apontamento>> ListarAsync(DateTime? de, DateTime? ate, CancellationToken cancellationToken)
        {
            Consultas++;
            return interno.ListarAsync(de, ate, cancellationToken);
        }

        public Task AdicionarAsync(Apontamento apontamento, CancellationToken cancellationToken) =>
            interno.AdicionarAsync(apontamento, cancellationToken);
    }

    private sealed class RecursoRepositorySpy : IRecursoRepository
    {
        private readonly RecursoRepositoryFake _interno = new();

        public int Consultas { get; private set; }

        public Task<Recurso?> ObterPorIdAsync(int id, CancellationToken cancellationToken) => _interno.ObterPorIdAsync(id, cancellationToken);

        public Task<Recurso?> ObterPorNomeAsync(string nome, CancellationToken cancellationToken) => _interno.ObterPorNomeAsync(nome, cancellationToken);

        public Task<IReadOnlyList<Recurso>> ListarAsync(CancellationToken cancellationToken)
        {
            Consultas++;
            return _interno.ListarAsync(cancellationToken);
        }

        public Task AdicionarAsync(Recurso recurso, CancellationToken cancellationToken) => _interno.AdicionarAsync(recurso, cancellationToken);

        public Task AtualizarAsync(Recurso recurso, CancellationToken cancellationToken) => _interno.AtualizarAsync(recurso, cancellationToken);
    }
}
