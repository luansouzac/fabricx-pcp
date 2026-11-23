using FabricX.Domain.Entities;

namespace FabricX.Application.DTOs.Estrutura;

public class ItemEstruturaDto
{
    public int Id { get; init; }
    public int ProdutoPaiId { get; init; }
    public int ComponenteId { get; init; }
    public string ComponenteCodigo { get; init; } = string.Empty;
    public string ComponenteNome { get; init; } = string.Empty;
    public string UnidadeMedida { get; init; } = string.Empty;
    public double Quantidade { get; init; }

    public static ItemEstruturaDto De(ItemEstrutura item, Produto componente) => new()
    {
        Id = item.Id,
        ProdutoPaiId = item.ProdutoPaiId,
        ComponenteId = item.ComponenteId,
        ComponenteCodigo = componente.Codigo,
        ComponenteNome = componente.Nome,
        UnidadeMedida = componente.UnidadeMedida,
        Quantidade = item.Quantidade
    };
}
