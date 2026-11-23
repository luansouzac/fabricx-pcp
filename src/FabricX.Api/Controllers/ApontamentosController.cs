using FabricX.Api.Common;
using FabricX.Application.DTOs.Apontamentos;
using FabricX.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FabricX.Api.Controllers;

[ApiController]
public class ApontamentosController : ControllerBase
{
    private readonly IApontamentoService _service;

    public ApontamentosController(IApontamentoService service)
    {
        _service = service;
    }

    [HttpPost("api/programacao/{programacaoId:int}/apontamentos")]
    [ProducesResponseType(typeof(WebResponse<ApontamentoDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Registrar(int programacaoId, [FromBody] RegistrarApontamentoDto dto, CancellationToken cancellationToken)
    {
        var apontamento = await _service.RegistrarAsync(programacaoId, dto, cancellationToken);
        var resposta = WebResponse<ApontamentoDto>.Success(apontamento, StatusCodes.Status201Created);
        return Created($"/api/programacao/{programacaoId}/apontamentos", resposta);
    }

    [HttpGet("api/programacao/{programacaoId:int}/apontamentos")]
    [ProducesResponseType(typeof(WebResponse<IReadOnlyList<ApontamentoDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListarPorProgramacao(int programacaoId, CancellationToken cancellationToken)
    {
        var apontamentos = await _service.ListarPorProgramacaoAsync(programacaoId, cancellationToken);
        return Ok(WebResponse<IReadOnlyList<ApontamentoDto>>.Success(apontamentos));
    }

    [HttpGet("api/ordens-producao/{ordemId:int}/apontamentos")]
    [ProducesResponseType(typeof(WebResponse<IReadOnlyList<ApontamentoDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListarPorOrdem(int ordemId, CancellationToken cancellationToken)
    {
        var apontamentos = await _service.ListarPorOrdemAsync(ordemId, cancellationToken);
        return Ok(WebResponse<IReadOnlyList<ApontamentoDto>>.Success(apontamentos));
    }
}
