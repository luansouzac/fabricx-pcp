using FabricX.Application.DTOs.Produtos;
using FabricX.Domain.Enums;

namespace FabricX.Application.DTOs.Indicadores;

public class OrdensPorStatusDto
{
    public StatusOrdem Status { get; init; }
    public int Quantidade { get; init; }
}

public class ResumoDto
{
    public IReadOnlyList<OrdensPorStatusDto> OrdensPorStatus { get; init; } = [];
    public IReadOnlyList<ProdutoDto> ProdutosComEstoqueBaixo { get; init; } = [];
    public IndicadoresDto Indicadores { get; init; } = new();
}
