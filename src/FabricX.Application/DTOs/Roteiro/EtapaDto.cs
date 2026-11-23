using FabricX.Domain.Entities;

namespace FabricX.Application.DTOs.Roteiro;

public class EtapaDto
{
    public int Id { get; init; }
    public int ProdutoId { get; init; }
    public int RecursoId { get; init; }
    public string RecursoNome { get; init; } = string.Empty;
    public int Sequencia { get; init; }
    public string Nome { get; init; } = string.Empty;
    public double TempoCicloMin { get; init; }
    public int UnidadesPorCiclo { get; init; }

    public static EtapaDto De(RoteiroEtapa etapa, Recurso recurso) => new()
    {
        Id = etapa.Id,
        ProdutoId = etapa.ProdutoId,
        RecursoId = etapa.RecursoId,
        RecursoNome = recurso.Nome,
        Sequencia = etapa.Sequencia,
        Nome = etapa.Nome,
        TempoCicloMin = etapa.TempoCicloMin,
        UnidadesPorCiclo = etapa.UnidadesPorCiclo
    };
}
