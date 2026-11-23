using FabricX.Domain.Entities;
using FabricX.Domain.Enums;
using Entidade = FabricX.Domain.Entities;

namespace FabricX.Application.DTOs.Apontamentos;

public class ApontamentoDto
{
    public int Id { get; init; }
    public int ProgramacaoId { get; init; }
    public int OrdemId { get; init; }
    public StatusProgramacao StatusProgramacao { get; init; }
    public DateTime DataHora { get; init; }
    public double QuantidadeProduzida { get; init; }
    public double QuantidadeDefeituosa { get; init; }
    public double TempoGastoMin { get; init; }
    public string? Observacao { get; init; }

    public static ApontamentoDto De(Apontamento apontamento, Entidade.Programacao programacao) => new()
    {
        Id = apontamento.Id,
        ProgramacaoId = apontamento.ProgramacaoId,
        OrdemId = programacao.OrdemId,
        StatusProgramacao = programacao.Status,
        DataHora = apontamento.DataHora,
        QuantidadeProduzida = apontamento.QtdProduzida,
        QuantidadeDefeituosa = apontamento.QtdDefeituosa,
        TempoGastoMin = apontamento.TempoGastoMin,
        Observacao = apontamento.Observacao
    };
}
