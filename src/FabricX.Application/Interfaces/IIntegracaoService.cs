using FabricX.Application.DTOs.Integracoes;

namespace FabricX.Application.Interfaces;

public interface IIntegracaoService
{
    Task<IReadOnlyList<PedidoErpDto>> ListarPedidosErpAsync(CancellationToken cancellationToken);
    Task<ConfirmacaoSgqDto> EnviarResultadoQualidadeAsync(ResultadoQualidadeDto dto, CancellationToken cancellationToken);
}
