using FabricX.Api.Common;
using FabricX.Application.DTOs.Capacidade;
using FabricX.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FabricX.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CapacidadeController : ControllerBase
{
    private readonly ICapacidadeService _service;

    public CapacidadeController(ICapacidadeService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(WebResponse<CapacidadeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Calcular([FromQuery] string? periodo, CancellationToken cancellationToken)
    {
        var capacidade = await _service.CalcularAsync(periodo, cancellationToken);
        return Ok(WebResponse<CapacidadeDto>.Success(capacidade));
    }
}
