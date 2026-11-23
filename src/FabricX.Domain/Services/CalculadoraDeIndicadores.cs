using FabricX.Domain.Entities;

namespace FabricX.Domain.Services;

public record ApontamentoComEtapa(Apontamento Apontamento, RoteiroEtapa Etapa);

public static class CalculadoraDeIndicadores
{
    public static double? Qualidade(IReadOnlyCollection<ApontamentoComEtapa> apontamentos)
    {
        var processada = apontamentos.Sum(a => a.Apontamento.QtdProcessada);
        return processada > 0 ? apontamentos.Sum(a => a.Apontamento.QtdDefeituosa) / processada : null;
    }

    public static double? Eficiencia(IReadOnlyCollection<ApontamentoComEtapa> apontamentos)
    {
        var gasto = apontamentos.Sum(a => a.Apontamento.TempoGastoMin);
        if (!(gasto > 0))
            return null;

        var padrao = apontamentos.Sum(a => a.Etapa.CalcularDuracaoMinutos(a.Apontamento.QtdProcessada));
        return padrao / gasto;
    }

    public static double? Produtividade(IReadOnlyCollection<ApontamentoComEtapa> apontamentos)
    {
        var horas = apontamentos.Sum(a => a.Apontamento.TempoGastoMin) / 60;
        return horas > 0 ? apontamentos.Sum(a => a.Apontamento.QtdProduzida) / horas : null;
    }

    public static double? CumprimentoDePrazo(IReadOnlyCollection<OrdemProducao> ordensConcluidas)
    {
        if (ordensConcluidas.Count == 0)
            return null;

        var noPrazo = ordensConcluidas.Count(o => DateOnly.FromDateTime(o.ConcluidaEm!.Value) <= o.PrazoEntrega);
        return (double)noPrazo / ordensConcluidas.Count;
    }
}
