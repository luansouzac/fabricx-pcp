using System.Globalization;
using FabricX.Domain.Entities;
using FabricX.Domain.Exceptions;

namespace FabricX.Domain.Services;

public static class RegrasDeProgramacao
{
    private static readonly CultureInfo Cultura = new("pt-BR");

    public static void ConferirSequencia(
        RoteiroEtapa etapa,
        DateTime inicio,
        DateTime fim,
        IReadOnlyList<RoteiroEtapa> roteiro,
        IReadOnlyList<Programacao> demaisDaOrdem)
    {
        var anteriores = roteiro.Where(e => e.Sequencia < etapa.Sequencia).OrderBy(e => e.Sequencia).ToList();

        foreach (var anterior in anteriores)
        {
            if (demaisDaOrdem.All(p => p.EtapaId != anterior.Id))
                throw new DomainException($"Programe antes a etapa {anterior.Nome} (sequência {anterior.Sequencia}).");
        }

        if (anteriores.Count > 0)
        {
            var anterior = anteriores[^1];
            var fimAnterior = demaisDaOrdem.Single(p => p.EtapaId == anterior.Id).FimPrevisto;
            if (inicio < fimAnterior)
                throw new DomainException($"A etapa {etapa.Nome} só pode começar depois do fim da etapa {anterior.Nome}, em {Formatar(fimAnterior)}.");
        }

        foreach (var posterior in roteiro.Where(e => e.Sequencia > etapa.Sequencia))
        {
            var programada = demaisDaOrdem.FirstOrDefault(p => p.EtapaId == posterior.Id);
            if (programada is not null && programada.InicioPrevisto < fim)
                throw new DomainException($"A etapa {posterior.Nome}, já programada, começa em {Formatar(programada.InicioPrevisto)}, antes do fim desta etapa.");
        }
    }

    public static void ConferirSemSobreposicao(Recurso recurso, DateTime inicio, DateTime fim, IReadOnlyList<Programacao> demaisDoRecurso)
    {
        var conflito = demaisDoRecurso.FirstOrDefault(p => inicio < p.FimPrevisto && fim > p.InicioPrevisto);
        if (conflito is not null)
            throw new DomainException($"O recurso {recurso.Nome} já está ocupado de {Formatar(conflito.InicioPrevisto)} a {Formatar(conflito.FimPrevisto)}.");
    }

    public static void ConferirCapacidade(Recurso recurso, DateTime inicio, DateTime fim, IReadOnlyList<Programacao> demaisDoRecurso)
    {
        var limiteMin = recurso.CapacidadeHorasDia * 60;

        for (var dia = inicio.Date; dia < fim; dia = dia.AddDays(1))
        {
            var novo = MinutosNoDia(inicio, fim, dia);
            var jaProgramado = demaisDoRecurso.Sum(p => MinutosNoDia(p.InicioPrevisto, p.FimPrevisto, dia));

            if (jaProgramado + novo > limiteMin)
                throw new DomainException(
                    $"O recurso {recurso.Nome} excederia a capacidade de {recurso.CapacidadeHorasDia} h em {dia.ToString("dd/MM/yyyy", Cultura)}: " +
                    $"já programado {jaProgramado / 60:0.##} h, solicitado {novo / 60:0.##} h.");
        }
    }

    private static double MinutosNoDia(DateTime inicio, DateTime fim, DateTime dia)
    {
        var de = inicio > dia ? inicio : dia;
        var ate = fim < dia.AddDays(1) ? fim : dia.AddDays(1);
        return ate > de ? (ate - de).TotalMinutes : 0;
    }

    private static string Formatar(DateTime data) => data.ToString("dd/MM/yyyy HH:mm", Cultura);
}
