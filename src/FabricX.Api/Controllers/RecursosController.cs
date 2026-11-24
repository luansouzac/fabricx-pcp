using FabricX.Api.Common;
using FabricX.Application.DTOs.Recursos;
using FabricX.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FabricX.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecursosController : ControllerBase
{
    private readonly IRecursoService _service;

    public RecursosController(IRecursoService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(WebResponse<IReadOnlyList<RecursoDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
    {
        var recursos = await _service.ListarAsync(cancellationToken);
        return Ok(WebResponse<IReadOnlyList<RecursoDto>>.Success(recursos));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(WebResponse<RecursoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(int id, CancellationToken cancellationToken)
    {
        var recurso = await _service.ObterPorIdAsync(id, cancellationToken);
        return Ok(WebResponse<RecursoDto>.Success(recurso));
    }

    [HttpPost]
    [ProducesResponseType(typeof(WebResponse<RecursoDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Criar([FromBody] CriarRecursoDto dto, CancellationToken cancellationToken)
    {
        var recurso = await _service.CriarAsync(dto, cancellationToken);
        var resposta = WebResponse<RecursoDto>.Success(recurso, StatusCodes.Status201Created);
        return CreatedAtAction(nameof(ObterPorId), new { id = recurso.Id }, resposta);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(WebResponse<RecursoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(int id, [FromBody] AtualizarRecursoDto dto, CancellationToken cancellationToken)
    {
        var recurso = await _service.AtualizarAsync(id, dto, cancellationToken);
        return Ok(WebResponse<RecursoDto>.Success(recurso));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Perfis.Administrador)]
    [ProducesResponseType(typeof(WebResponse<RecursoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Inativar(int id, CancellationToken cancellationToken)
    {
        var recurso = await _service.InativarAsync(id, cancellationToken);
        return Ok(WebResponse<RecursoDto>.Success(recurso));
    }
}
