namespace FabricX.Application.DTOs.Integracoes;

public class PedidoErpDto
{
    public string Numero { get; init; } = string.Empty;
    public string ProdutoCodigo { get; init; } = string.Empty;
    public double Quantidade { get; init; }
    public DateOnly PrazoEntrega { get; init; }
}
