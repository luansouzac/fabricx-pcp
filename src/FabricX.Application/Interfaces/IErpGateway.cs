using FabricX.Application.DTOs.Integracoes;

namespace FabricX.Application.Interfaces;

public interface IErpGateway
{
    Task<IReadOnlyList<PedidoErpDto>> ObterPedidosPendentesAsync(CancellationToken cancellationToken);
}
