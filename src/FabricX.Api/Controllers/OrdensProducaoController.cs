using FabricX.Api.Common;
using FabricX.Application.DTOs.Ordens;
using FabricX.Application.Interfaces;
using FabricX.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace FabricX.Api.Controllers;

[ApiController]
[Route("api/ordens-producao")]
public class OrdensProducaoController : ControllerBase
{
    private readonly IOrdemProducaoService _service;

    public OrdensProducaoController(IOrdemProducaoService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(WebResponse<IReadOnlyList<OrdemProducaoDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Listar([FromQuery] StatusOrdem? status, CancellationToken cancellationToken)
    {
        var ordens = await _service.ListarAsync(status, cancellationToken);
        return Ok(WebResponse<IReadOnlyList<OrdemProducaoDto>>.Success(ordens));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(WebResponse<OrdemProducaoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(int id, CancellationToken cancellationToken)
    {
        var ordem = await _service.ObterPorIdAsync(id, cancellationToken);
        return Ok(WebResponse<OrdemProducaoDto>.Success(ordem));
    }

    [HttpPost]
    [ProducesResponseType(typeof(WebResponse<OrdemProducaoDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Criar([FromBody] CriarOrdemProducaoDto dto, CancellationToken cancellationToken)
    {
        var ordem = await _service.CriarAsync(dto, cancellationToken);
        var resposta = WebResponse<OrdemProducaoDto>.Success(ordem, StatusCodes.Status201Created);
        return CreatedAtAction(nameof(ObterPorId), new { id = ordem.Id }, resposta);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(WebResponse<OrdemProducaoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Alterar(int id, [FromBody] AlterarOrdemProducaoDto dto, CancellationToken cancellationToken)
    {
        var ordem = await _service.AlterarAsync(id, dto, cancellationToken);
        return Ok(WebResponse<OrdemProducaoDto>.Success(ordem));
    }

    [HttpPost("{id:int}/avancar")]
    [ProducesResponseType(typeof(WebResponse<OrdemProducaoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Avancar(int id, CancellationToken cancellationToken)
    {
        var ordem = await _service.AvancarAsync(id, cancellationToken);
        return Ok(WebResponse<OrdemProducaoDto>.Success(ordem));
    }

    [HttpPost("{id:int}/cancelar")]
    [ProducesResponseType(typeof(WebResponse<OrdemProducaoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancelar(int id, CancellationToken cancellationToken)
    {
        var ordem = await _service.CancelarAsync(id, cancellationToken);
        return Ok(WebResponse<OrdemProducaoDto>.Success(ordem));
    }
}
