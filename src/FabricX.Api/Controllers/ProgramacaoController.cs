using FabricX.Api.Common;
using FabricX.Application.DTOs.Programacao;
using FabricX.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FabricX.Api.Controllers;

[ApiController]
public class ProgramacaoController : ControllerBase
{
    private readonly IProgramacaoService _service;

    public ProgramacaoController(IProgramacaoService service)
    {
        _service = service;
    }

    [HttpPost("api/ordens-producao/{ordemId:int}/programacao")]
    [ProducesResponseType(typeof(WebResponse<ProgramacaoDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Agendar(int ordemId, [FromBody] AgendarEtapaDto dto, CancellationToken cancellationToken)
    {
        var programacao = await _service.AgendarAsync(ordemId, dto, cancellationToken);
        var resposta = WebResponse<ProgramacaoDto>.Success(programacao, StatusCodes.Status201Created);
        return Created($"/api/ordens-producao/{ordemId}/programacao", resposta);
    }

    [HttpGet("api/ordens-producao/{ordemId:int}/programacao")]
    [ProducesResponseType(typeof(WebResponse<IReadOnlyList<ProgramacaoDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListarPorOrdem(int ordemId, CancellationToken cancellationToken)
    {
        var programacoes = await _service.ListarPorOrdemAsync(ordemId, cancellationToken);
        return Ok(WebResponse<IReadOnlyList<ProgramacaoDto>>.Success(programacoes));
    }

    [HttpGet("api/programacao")]
    [ProducesResponseType(typeof(WebResponse<IReadOnlyList<ProgramacaoDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ListarCronograma([FromQuery] int? recursoId, [FromQuery] DateOnly? de, [FromQuery] DateOnly? ate, CancellationToken cancellationToken)
    {
        var programacoes = await _service.ListarCronogramaAsync(recursoId, de, ate, cancellationToken);
        return Ok(WebResponse<IReadOnlyList<ProgramacaoDto>>.Success(programacoes));
    }

    [HttpPut("api/programacao/{id:int}")]
    [ProducesResponseType(typeof(WebResponse<ProgramacaoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reprogramar(int id, [FromBody] ReprogramarEtapaDto dto, CancellationToken cancellationToken)
    {
        var programacao = await _service.ReprogramarAsync(id, dto, cancellationToken);
        return Ok(WebResponse<ProgramacaoDto>.Success(programacao));
    }

    [HttpDelete("api/programacao/{id:int}")]
    [ProducesResponseType(typeof(WebResponse<ProgramacaoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover(int id, CancellationToken cancellationToken)
    {
        var programacao = await _service.RemoverAsync(id, cancellationToken);
        return Ok(WebResponse<ProgramacaoDto>.Success(programacao));
    }
}
