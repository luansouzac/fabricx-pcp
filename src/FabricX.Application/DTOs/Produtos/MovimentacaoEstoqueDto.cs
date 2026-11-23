using FabricX.Domain.Entities;
using FabricX.Domain.Enums;

namespace FabricX.Application.DTOs.Produtos;

public class MovimentacaoEstoqueDto
{
    public int Id { get; init; }
    public int ProdutoId { get; init; }
    public int? OrdemId { get; init; }
    public TipoMovimentacao Tipo { get; init; }
    public double Quantidade { get; init; }
    public string? Motivo { get; init; }
    public DateTime DataHora { get; init; }

    public static MovimentacaoEstoqueDto De(MovimentacaoEstoque movimentacao) => new()
    {
        Id = movimentacao.Id,
        ProdutoId = movimentacao.ProdutoId,
        OrdemId = movimentacao.OrdemId,
        Tipo = movimentacao.Tipo,
        Quantidade = movimentacao.Quantidade,
        Motivo = movimentacao.Motivo,
        DataHora = movimentacao.DataHora
    };
}
