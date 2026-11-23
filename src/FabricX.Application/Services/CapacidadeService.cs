using FabricX.Application.DTOs.Capacidade;
using FabricX.Application.Interfaces;
using FabricX.Domain.Entities;
using FabricX.Domain.Services;
using FabricX.Domain.ValueObjects;

namespace FabricX.Application.Services;

public class CapacidadeService : ICapacidadeService
{
    private readonly IRecursoRepository _recursos;
    private readonly IProgramacaoRepository _programacoes;
    private readonly IDemandaPrevistaRepository _demandas;
    private readonly IRoteiroEtapaRepository _etapas;

    public CapacidadeService(
        IRecursoRepository recursos,
        IProgramacaoRepository programacoes,
        IDemandaPrevistaRepository demandas,
        IRoteiroEtapaRepository etapas)
    {
        _recursos = recursos;
        _programacoes = programacoes;
        _demandas = demandas;
        _etapas = etapas;
    }

    public async Task<CapacidadeDto> CalcularAsync(string? periodoTexto, CancellationToken cancellationToken)
    {
        var periodo = periodoTexto is null ? PeriodoMensal.Atual() : PeriodoMensal.Parse(periodoTexto);

        var recursos = (await _recursos.ListarAsync(cancellationToken)).Where(r => r.Ativo).ToList();
        var programacoes = await _programacoes.ListarAsync(null, periodo.Inicio, periodo.Fim, cancellationToken);

        var demandas = new List<(DemandaPrevista, IReadOnlyList<RoteiroEtapa>)>();
        foreach (var demanda in await _demandas.ListarAsync(periodo.ToString(), cancellationToken))
            demandas.Add((demanda, await _etapas.ListarPorProdutoAsync(demanda.ProdutoId, cancellationToken)));

        var cargas = CalculadoraDeCapacidade.Calcular(periodo, recursos, programacoes, demandas);

        return new CapacidadeDto
        {
            Periodo = periodo.ToString(),
            DiasNoPeriodo = periodo.Dias,
            Recursos = cargas.Select(c => new CapacidadeRecursoDto
            {
                RecursoId = c.Recurso.Id,
                RecursoNome = c.Recurso.Nome,
                CapacidadeHorasPeriodo = c.CapacidadeHorasPeriodo,
                HorasProgramadas = c.HorasProgramadas,
                PercentualProgramado = c.PercentualProgramado,
                HorasNecessariasDemanda = c.HorasNecessariasDemanda,
                PercentualDemanda = c.PercentualDemanda,
                PercentualCarga = c.PercentualCarga,
                Classificacao = c.Classificacao
            }).ToList()
        };
    }
}
