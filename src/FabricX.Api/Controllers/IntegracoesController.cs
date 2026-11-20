using FabricX.Api.Common;
using FabricX.Application.DTOs.Integracoes;
using FabricX.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FabricX.Api.Controllers;

[ApiController]
[Route("api/integracoes")]
public class IntegracoesController : ControllerBase
{
    private readonly IIntegracaoService _service;

    public IntegracoesController(IIntegracaoService service)
    {
        _service = service;
    }

    [HttpGet("erp/pedidos")]
    [ProducesResponseType(typeof(WebResponse<IReadOnlyList<PedidoErpDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarPedidosErp(CancellationToken cancellationToken)
    {
        var pedidos = await _service.ListarPedidosErpAsync(cancellationToken);
        return Ok(WebResponse<IReadOnlyList<PedidoErpDto>>.Success(pedidos));
    }

    [HttpPost("sgq/resultados")]
    [ProducesResponseType(typeof(WebResponse<ConfirmacaoSgqDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EnviarResultadoQualidade([FromBody] ResultadoQualidadeDto dto, CancellationToken cancellationToken)
    {
        var confirmacao = await _service.EnviarResultadoQualidadeAsync(dto, cancellationToken);
        return Ok(WebResponse<ConfirmacaoSgqDto>.Success(confirmacao));
    }
}
