using FabricX.Application.DTOs.Integracoes;

namespace FabricX.Application.Interfaces;

public interface ISgqGateway
{
    Task<ConfirmacaoSgqDto> EnviarResultadoAsync(ResultadoQualidadeDto resultado, CancellationToken cancellationToken);
}
