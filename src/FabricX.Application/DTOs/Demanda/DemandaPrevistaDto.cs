using FabricX.Domain.Entities;

namespace FabricX.Application.DTOs.Demanda;

public class DemandaPrevistaDto
{
    public int Id { get; init; }
    public int ProdutoId { get; init; }
    public string ProdutoCodigo { get; init; } = string.Empty;
    public string ProdutoNome { get; init; } = string.Empty;
    public string Periodo { get; init; } = string.Empty;
    public double Quantidade { get; init; }

    public static DemandaPrevistaDto De(DemandaPrevista demanda, Produto produto) => new()
    {
        Id = demanda.Id,
        ProdutoId = demanda.ProdutoId,
        ProdutoCodigo = produto.Codigo,
        ProdutoNome = produto.Nome,
        Periodo = demanda.Periodo,
        Quantidade = demanda.Quantidade
    };
}
