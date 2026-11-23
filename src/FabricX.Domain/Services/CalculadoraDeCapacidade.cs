using FabricX.Domain.Entities;
using FabricX.Domain.Enums;
using FabricX.Domain.ValueObjects;

namespace FabricX.Domain.Services;

public record CargaDoRecurso(
    Recurso Recurso,
    double CapacidadeHorasPeriodo,
    double HorasProgramadas,
    double HorasNecessariasDemanda)
{
    public double PercentualProgramado => Percentual(HorasProgramadas);
    public double PercentualDemanda => Percentual(HorasNecessariasDemanda);
    public double PercentualCarga => Math.Max(PercentualProgramado, PercentualDemanda);

    public ClassificacaoCarga Classificacao => PercentualCarga switch
    {
        > 100 => ClassificacaoCarga.Gargalo,
        < 50 => ClassificacaoCarga.Ocioso,
        _ => ClassificacaoCarga.Normal
    };

    private double Percentual(double horas) => CapacidadeHorasPeriodo > 0 ? horas / CapacidadeHorasPeriodo * 100 : 0;
}

public static class CalculadoraDeCapacidade
{
    public static IReadOnlyList<CargaDoRecurso> Calcular(
        PeriodoMensal periodo,
        IReadOnlyList<Recurso> recursos,
        IReadOnlyList<Programacao> programacoes,
        IReadOnlyList<(DemandaPrevista Demanda, IReadOnlyList<RoteiroEtapa> Roteiro)> demandas)
    {
        return recursos
            .Select(recurso => new CargaDoRecurso(
                recurso,
                recurso.CapacidadeHorasDia * periodo.Dias,
                HorasProgramadas(periodo, recurso, programacoes),
                HorasNecessarias(recurso, demandas)))
            .ToList();
    }

    private static double HorasProgramadas(PeriodoMensal periodo, Recurso recurso, IReadOnlyList<Programacao> programacoes)
    {
        var minutos = programacoes
            .Where(p => p.RecursoId == recurso.Id)
            .Sum(p =>
            {
                var de = p.InicioPrevisto > periodo.Inicio ? p.InicioPrevisto : periodo.Inicio;
                var ate = p.FimPrevisto < periodo.Fim ? p.FimPrevisto : periodo.Fim;
                return ate > de ? (ate - de).TotalMinutes : 0;
            });

        return minutos / 60;
    }

    private static double HorasNecessarias(Recurso recurso, IReadOnlyList<(DemandaPrevista Demanda, IReadOnlyList<RoteiroEtapa> Roteiro)> demandas)
    {
        var minutos = demandas.Sum(d => d.Roteiro
            .Where(e => e.RecursoId == recurso.Id)
            .Sum(e => e.CalcularDuracaoMinutos(d.Demanda.Quantidade)));

        return minutos / 60;
    }
}
