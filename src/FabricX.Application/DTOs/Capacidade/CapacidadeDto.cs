using FabricX.Domain.Enums;

namespace FabricX.Application.DTOs.Capacidade;

public class CapacidadeRecursoDto
{
    public int RecursoId { get; init; }
    public string RecursoNome { get; init; } = string.Empty;
    public double CapacidadeHorasPeriodo { get; init; }
    public double HorasProgramadas { get; init; }
    public double PercentualProgramado { get; init; }
    public double HorasNecessariasDemanda { get; init; }
    public double PercentualDemanda { get; init; }
    public double PercentualCarga { get; init; }
    public ClassificacaoCarga Classificacao { get; init; }
}

public class CapacidadeDto
{
    public string Periodo { get; init; } = string.Empty;
    public int DiasNoPeriodo { get; init; }
    public IReadOnlyList<CapacidadeRecursoDto> Recursos { get; init; } = [];
}
