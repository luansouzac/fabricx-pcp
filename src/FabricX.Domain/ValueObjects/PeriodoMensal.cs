using System.Globalization;
using System.Text.RegularExpressions;
using FabricX.Domain.Exceptions;

namespace FabricX.Domain.ValueObjects;

public readonly partial record struct PeriodoMensal
{
    public int Ano { get; }
    public int Mes { get; }

    public DateTime Inicio => new(Ano, Mes, 1);
    public DateTime Fim => Inicio.AddMonths(1);
    public int Dias => DateTime.DaysInMonth(Ano, Mes);

    private PeriodoMensal(int ano, int mes)
    {
        Ano = ano;
        Mes = mes;
    }

    public static PeriodoMensal Parse(string? texto)
    {
        var correspondencia = texto is null ? null : FormatoPeriodo().Match(texto.Trim());
        if (correspondencia is null || !correspondencia.Success)
            throw new DomainException("O período deve estar no formato AAAA-MM, com mês de 01 a 12.");

        return new PeriodoMensal(int.Parse(correspondencia.Groups[1].Value, CultureInfo.InvariantCulture), int.Parse(correspondencia.Groups[2].Value, CultureInfo.InvariantCulture));
    }

    public static PeriodoMensal Atual() => new(DateTime.Today.Year, DateTime.Today.Month);

    public override string ToString() => $"{Ano:0000}-{Mes:00}";

    [GeneratedRegex(@"^(\d{4})-(0[1-9]|1[0-2])$")]
    private static partial Regex FormatoPeriodo();
}
