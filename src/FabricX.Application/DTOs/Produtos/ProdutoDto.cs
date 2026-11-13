using FabricX.Domain.Entities;
using FabricX.Domain.Enums;

namespace FabricX.Application.DTOs.Produtos;

public class ProdutoDto
{
    public int Id { get; init; }
    public string Codigo { get; init; } = string.Empty;
    public string Nome { get; init; } = string.Empty;
    public string? Descricao { get; init; }
    public TipoProduto Tipo { get; init; }
    public string UnidadeMedida { get; init; } = string.Empty;
    public double EstoqueAtual { get; init; }
    public double EstoqueMinimo { get; init; }
    public double CustoUnitario { get; init; }
    public bool Ativo { get; init; }
    public bool EstoqueAbaixoDoMinimo { get; init; }

    public static ProdutoDto De(Produto produto) => new()
    {
        Id = produto.Id,
        Codigo = produto.Codigo,
        Nome = produto.Nome,
        Descricao = produto.Descricao,
        Tipo = produto.Tipo,
        UnidadeMedida = produto.UnidadeMedida,
        EstoqueAtual = produto.EstoqueAtual,
        EstoqueMinimo = produto.EstoqueMinimo,
        CustoUnitario = produto.CustoUnitario,
        Ativo = produto.Ativo,
        EstoqueAbaixoDoMinimo = produto.EstoqueAbaixoDoMinimo
    };
}
