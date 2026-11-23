using FabricX.Api.Common;
using FabricX.Application.DTOs.Roteiro;
using FabricX.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FabricX.Api.Controllers;

[ApiController]
[Route("api/produtos/{produtoId:int}/roteiro")]
public class RoteiroController : ControllerBase
{
    private readonly IRoteiroService _service;

    public RoteiroController(IRoteiroService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(WebResponse<IReadOnlyList<EtapaDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Listar(int produtoId, CancellationToken cancellationToken)
    {
        var etapas = await _service.ListarAsync(produtoId, cancellationToken);
        return Ok(WebResponse<IReadOnlyList<EtapaDto>>.Success(etapas));
    }

    [HttpPost]
    [ProducesResponseType(typeof(WebResponse<EtapaDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Adicionar(int produtoId, [FromBody] CriarEtapaDto dto, CancellationToken cancellationToken)
    {
        var etapa = await _service.AdicionarAsync(produtoId, dto, cancellationToken);
        var resposta = WebResponse<EtapaDto>.Success(etapa, StatusCodes.Status201Created);
        return Created($"/api/produtos/{produtoId}/roteiro", resposta);
    }

    [HttpPut("{etapaId:int}")]
    [ProducesResponseType(typeof(WebResponse<EtapaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(int produtoId, int etapaId, [FromBody] AtualizarEtapaDto dto, CancellationToken cancellationToken)
    {
        var etapa = await _service.AtualizarAsync(produtoId, etapaId, dto, cancellationToken);
        return Ok(WebResponse<EtapaDto>.Success(etapa));
    }

    [HttpDelete("{etapaId:int}")]
    [ProducesResponseType(typeof(WebResponse<EtapaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover(int produtoId, int etapaId, CancellationToken cancellationToken)
    {
        var etapa = await _service.RemoverAsync(produtoId, etapaId, cancellationToken);
        return Ok(WebResponse<EtapaDto>.Success(etapa));
    }
}
