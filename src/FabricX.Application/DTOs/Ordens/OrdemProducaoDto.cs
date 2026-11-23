using FabricX.Domain.Entities;
using FabricX.Domain.Enums;

namespace FabricX.Application.DTOs.Ordens;

public class OrdemProducaoDto
{
    public int Id { get; init; }
    public string Numero { get; init; } = string.Empty;
    public int ProdutoId { get; init; }
    public string ProdutoCodigo { get; init; } = string.Empty;
    public string ProdutoNome { get; init; } = string.Empty;
    public double Quantidade { get; init; }
    public DateOnly PrazoEntrega { get; init; }
    public Prioridade Prioridade { get; init; }
    public StatusOrdem Status { get; init; }
    public DateTime CriadaEm { get; init; }
    public DateTime? ConcluidaEm { get; init; }

    public static OrdemProducaoDto De(OrdemProducao ordem, Produto produto) => new()
    {
        Id = ordem.Id,
        Numero = ordem.Numero,
        ProdutoId = ordem.ProdutoId,
        ProdutoCodigo = produto.Codigo,
        ProdutoNome = produto.Nome,
        Quantidade = ordem.Quantidade,
        PrazoEntrega = ordem.PrazoEntrega,
        Prioridade = ordem.Prioridade,
        Status = ordem.Status,
        CriadaEm = ordem.CriadaEm,
        ConcluidaEm = ordem.ConcluidaEm
    };
}
