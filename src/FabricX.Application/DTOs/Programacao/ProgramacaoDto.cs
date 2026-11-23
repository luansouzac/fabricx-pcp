using FabricX.Domain.Enums;
using Entidade = FabricX.Domain.Entities;

namespace FabricX.Application.DTOs.Programacao;

public class ProgramacaoDto
{
    public int Id { get; init; }
    public int OrdemId { get; init; }
    public string OrdemNumero { get; init; } = string.Empty;
    public int EtapaId { get; init; }
    public string EtapaNome { get; init; } = string.Empty;
    public int Sequencia { get; init; }
    public int RecursoId { get; init; }
    public string RecursoNome { get; init; } = string.Empty;
    public DateTime InicioPrevisto { get; init; }
    public DateTime FimPrevisto { get; init; }
    public StatusProgramacao Status { get; init; }

    public static ProgramacaoDto De(Entidade.Programacao programacao, Entidade.OrdemProducao ordem, Entidade.RoteiroEtapa etapa, Entidade.Recurso recurso) => new()
    {
        Id = programacao.Id,
        OrdemId = ordem.Id,
        OrdemNumero = ordem.Numero,
        EtapaId = etapa.Id,
        EtapaNome = etapa.Nome,
        Sequencia = etapa.Sequencia,
        RecursoId = recurso.Id,
        RecursoNome = recurso.Nome,
        InicioPrevisto = programacao.InicioPrevisto,
        FimPrevisto = programacao.FimPrevisto,
        Status = programacao.Status
    };
}
