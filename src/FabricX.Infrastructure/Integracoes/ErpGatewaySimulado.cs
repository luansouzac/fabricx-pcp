using FabricX.Application.DTOs.Integracoes;
using FabricX.Application.Interfaces;

namespace FabricX.Infrastructure.Integracoes;

public class ErpGatewaySimulado : IErpGateway
{
    public Task<IReadOnlyList<PedidoErpDto>> ObterPedidosPendentesAsync(CancellationToken cancellationToken)
    {
        var hoje = DateOnly.FromDateTime(DateTime.Today);

        IReadOnlyList<PedidoErpDto> pedidos =
        [
            new() { Numero = "ERP-1001", ProdutoCodigo = "PA-001", Quantidade = 20, PrazoEntrega = hoje.AddDays(15) },
            new() { Numero = "ERP-1002", ProdutoCodigo = "PA-002", Quantidade = 8, PrazoEntrega = hoje.AddDays(30) }
        ];

        return Task.FromResult(pedidos);
    }
}
