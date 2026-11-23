using FabricX.Application.DTOs.Indicadores;
using FabricX.Application.DTOs.Produtos;
using FabricX.Application.Interfaces;
using FabricX.Domain.Entities;
using FabricX.Domain.Enums;
using FabricX.Domain.Services;

namespace FabricX.Application.Services;

public class IndicadorService : IIndicadorService
{
    private readonly IApontamentoRepository _apontamentos;
    private readonly IProgramacaoRepository _programacoes;
    private readonly IOrdemProducaoRepository _ordens;
    private readonly IRoteiroEtapaRepository _etapas;
    private readonly IProdutoRepository _produtos;

    public IndicadorService(
        IApontamentoRepository apontamentos,
        IProgramacaoRepository programacoes,
        IOrdemProducaoRepository ordens,
        IRoteiroEtapaRepository etapas,
        IProdutoRepository produtos)
    {
        _apontamentos = apontamentos;
        _programacoes = programacoes;
        _ordens = ordens;
        _etapas = etapas;
        _produtos = produtos;
    }

    public async Task<IndicadoresDto> CalcularAsync(DateOnly? de, DateOnly? ate, int? produtoId, int? recursoId, CancellationToken cancellationToken)
    {
        var inicio = de?.ToDateTime(TimeOnly.MinValue);
        var fim = ate?.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var todasOrdens = await _ordens.ListarAsync(null, cancellationToken);
        var programacoes = (await _programacoes.ListarAsync(null, null, null, cancellationToken)).ToDictionary(p => p.Id);
        var ordens = todasOrdens.ToDictionary(o => o.Id);
        var etapas = await CarregarEtapasAsync(todasOrdens, cancellationToken);

        var apontamentos = (await _apontamentos.ListarAsync(inicio, fim, cancellationToken))
            .Where(a => programacoes.TryGetValue(a.ProgramacaoId, out var p)
                && (produtoId is null || ordens[p.OrdemId].ProdutoId == produtoId)
                && (recursoId is null || p.RecursoId == recursoId))
            .Select(a => new ApontamentoComEtapa(a, etapas[programacoes[a.ProgramacaoId].EtapaId]))
            .ToList();

        var recursosDasOrdens = programacoes.Values
            .Where(p => recursoId is null || p.RecursoId == recursoId)
            .Select(p => p.OrdemId)
            .ToHashSet();

        var concluidas = todasOrdens
            .Where(o => o.Status == StatusOrdem.Concluida && o.ConcluidaEm is not null)
            .Where(o => produtoId is null || o.ProdutoId == produtoId)
            .Where(o => recursoId is null || recursosDasOrdens.Contains(o.Id))
            .Where(o => inicio is null || o.ConcluidaEm >= inicio)
            .Where(o => fim is null || o.ConcluidaEm < fim)
            .ToList();

        return new IndicadoresDto
        {
            Qualidade = CalculadoraDeIndicadores.Qualidade(apontamentos),
            Eficiencia = CalculadoraDeIndicadores.Eficiencia(apontamentos),
            ProdutividadePorHora = CalculadoraDeIndicadores.Produtividade(apontamentos),
            CumprimentoDePrazo = CalculadoraDeIndicadores.CumprimentoDePrazo(concluidas),
            TotalProduzido = apontamentos.Sum(a => a.Apontamento.QtdProduzida),
            TotalDefeituoso = apontamentos.Sum(a => a.Apontamento.QtdDefeituosa),
            TempoApontadoMin = apontamentos.Sum(a => a.Apontamento.TempoGastoMin),
            OrdensConcluidas = concluidas.Count
        };
    }

    public async Task<ResumoDto> ObterResumoAsync(CancellationToken cancellationToken)
    {
        var ordens = await _ordens.ListarAsync(null, cancellationToken);
        var produtos = await _produtos.ListarAsync(null, cancellationToken);

        return new ResumoDto
        {
            OrdensPorStatus = Enum.GetValues<StatusOrdem>()
                .Select(s => new OrdensPorStatusDto { Status = s, Quantidade = ordens.Count(o => o.Status == s) })
                .ToList(),
            ProdutosComEstoqueBaixo = produtos.Where(p => p.Ativo && p.EstoqueAbaixoDoMinimo).Select(ProdutoDto.De).ToList(),
            Indicadores = await CalcularAsync(null, null, null, null, cancellationToken)
        };
    }

    private async Task<Dictionary<int, RoteiroEtapa>> CarregarEtapasAsync(IReadOnlyList<OrdemProducao> ordens, CancellationToken cancellationToken)
    {
        var etapas = new Dictionary<int, RoteiroEtapa>();

        foreach (var produtoId in ordens.Select(o => o.ProdutoId).Distinct())
        {
            foreach (var etapa in await _etapas.ListarPorProdutoAsync(produtoId, cancellationToken))
                etapas[etapa.Id] = etapa;
        }

        return etapas;
    }
}
