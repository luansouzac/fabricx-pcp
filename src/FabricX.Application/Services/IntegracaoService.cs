using FabricX.Application.DTOs.Integracoes;
using FabricX.Application.Interfaces;
using FabricX.Domain.Exceptions;

namespace FabricX.Application.Services;

public class IntegracaoService : IIntegracaoService
{
    private readonly IErpGateway _erp;
    private readonly ISgqGateway _sgq;

    public IntegracaoService(IErpGateway erp, ISgqGateway sgq)
    {
        _erp = erp;
        _sgq = sgq;
    }

    public Task<IReadOnlyList<PedidoErpDto>> ListarPedidosErpAsync(CancellationToken cancellationToken) =>
        _erp.ObterPedidosPendentesAsync(cancellationToken);

    public Task<ConfirmacaoSgqDto> EnviarResultadoQualidadeAsync(ResultadoQualidadeDto dto, CancellationToken cancellationToken)
    {
        if (dto.QuantidadeDefeituosa > dto.QuantidadeInspecionada)
            throw new DomainException("A quantidade defeituosa não pode ser maior que a inspecionada.");

        return _sgq.EnviarResultadoAsync(dto, cancellationToken);
    }
}
