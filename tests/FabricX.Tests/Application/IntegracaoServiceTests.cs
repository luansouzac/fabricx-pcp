using FabricX.Application.DTOs.Integracoes;
using FabricX.Application.Services;
using FabricX.Domain.Exceptions;
using FabricX.Infrastructure.Integracoes;

namespace FabricX.Tests.Application;

public class IntegracaoServiceTests
{
    private static readonly CancellationToken Ct = CancellationToken.None;

    private readonly IntegracaoService _service = new(new ErpGatewaySimulado(), new SgqGatewaySimulado());

    [Fact]
    public async Task ListarPedidosErp_DevolveOsPedidosDaImplementacaoSimulada()
    {
        var pedidos = await _service.ListarPedidosErpAsync(Ct);

        Assert.Equal(["ERP-1001", "ERP-1002"], pedidos.Select(p => p.Numero));
        Assert.All(pedidos, p => Assert.True(p.PrazoEntrega >= DateOnly.FromDateTime(DateTime.Today)));
    }

    [Fact]
    public async Task EnviarResultadoQualidade_DevolveProtocoloETaxaDeDefeitos()
    {
        var confirmacao = await _service.EnviarResultadoQualidadeAsync(
            new ResultadoQualidadeDto { OrdemNumero = "OP-2026-0001", QuantidadeInspecionada = 50, QuantidadeDefeituosa = 5 }, Ct);

        Assert.Equal("SGQ-SIM-OP-2026-0001", confirmacao.Protocolo);
        Assert.Equal(0.1, confirmacao.TaxaDefeitos);
    }

    [Fact]
    public async Task EnviarResultadoQualidade_DefeitosMaioresQueInspecionados_LancaDomainException()
    {
        await Assert.ThrowsAsync<DomainException>(() => _service.EnviarResultadoQualidadeAsync(
            new ResultadoQualidadeDto { OrdemNumero = "OP-2026-0001", QuantidadeInspecionada = 5, QuantidadeDefeituosa = 6 }, Ct));
    }
}
