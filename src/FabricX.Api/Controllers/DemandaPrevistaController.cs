using FabricX.Api.Common;
using FabricX.Application.DTOs.Demanda;
using FabricX.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FabricX.Api.Controllers;

[ApiController]
[Route("api/demanda-prevista")]
public class DemandaPrevistaController : ControllerBase
{
    private readonly IDemandaPrevistaService _service;

    public DemandaPrevistaController(IDemandaPrevistaService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(WebResponse<IReadOnlyList<DemandaPrevistaDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Listar([FromQuery] string? periodo, CancellationToken cancellationToken)
    {
        var demandas = await _service.ListarAsync(periodo, cancellationToken);
        return Ok(WebResponse<IReadOnlyList<DemandaPrevistaDto>>.Success(demandas));
    }

    [HttpPost]
    [ProducesResponseType(typeof(WebResponse<DemandaPrevistaDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Criar([FromBody] CriarDemandaPrevistaDto dto, CancellationToken cancellationToken)
    {
        var demanda = await _service.CriarAsync(dto, cancellationToken);
        var resposta = WebResponse<DemandaPrevistaDto>.Success(demanda, StatusCodes.Status201Created);
        return Created($"/api/demanda-prevista?periodo={demanda.Periodo}", resposta);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(WebResponse<DemandaPrevistaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(int id, [FromBody] AtualizarDemandaPrevistaDto dto, CancellationToken cancellationToken)
    {
        var demanda = await _service.AtualizarAsync(id, dto, cancellationToken);
        return Ok(WebResponse<DemandaPrevistaDto>.Success(demanda));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(WebResponse<DemandaPrevistaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover(int id, CancellationToken cancellationToken)
    {
        var demanda = await _service.RemoverAsync(id, cancellationToken);
        return Ok(WebResponse<DemandaPrevistaDto>.Success(demanda));
    }
}
