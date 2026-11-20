using FabricX.Application.DTOs.Integracoes;
using FabricX.Application.Interfaces;

namespace FabricX.Infrastructure.Integracoes;

public class SgqGatewaySimulado : ISgqGateway
{
    public Task<ConfirmacaoSgqDto> EnviarResultadoAsync(ResultadoQualidadeDto resultado, CancellationToken cancellationToken)
    {
        var confirmacao = new ConfirmacaoSgqDto
        {
            Protocolo = $"SGQ-SIM-{resultado.OrdemNumero}",
            TaxaDefeitos = resultado.QuantidadeDefeituosa / resultado.QuantidadeInspecionada
        };

        return Task.FromResult(confirmacao);
    }
}
