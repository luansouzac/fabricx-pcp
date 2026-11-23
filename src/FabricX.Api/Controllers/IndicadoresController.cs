using FabricX.Api.Common;
using FabricX.Application.DTOs.Indicadores;
using FabricX.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FabricX.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IndicadoresController : ControllerBase
{
    private readonly IIndicadorService _service;

    public IndicadoresController(IIndicadorService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(WebResponse<IndicadoresDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Calcular([FromQuery] DateOnly? de, [FromQuery] DateOnly? ate, [FromQuery] int? produtoId, [FromQuery] int? recursoId, CancellationToken cancellationToken)
    {
        var indicadores = await _service.CalcularAsync(de, ate, produtoId, recursoId, cancellationToken);
        return Ok(WebResponse<IndicadoresDto>.Success(indicadores));
    }

    [HttpGet("resumo")]
    [ProducesResponseType(typeof(WebResponse<ResumoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterResumo(CancellationToken cancellationToken)
    {
        var resumo = await _service.ObterResumoAsync(cancellationToken);
        return Ok(WebResponse<ResumoDto>.Success(resumo));
    }
}
