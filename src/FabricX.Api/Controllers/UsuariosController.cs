using FabricX.Api.Common;
using FabricX.Application.DTOs.Auth;
using FabricX.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FabricX.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Perfis.Administrador)]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _service;

    public UsuariosController(IUsuarioService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(WebResponse<IReadOnlyList<UsuarioDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
    {
        var usuarios = await _service.ListarAsync(cancellationToken);
        return Ok(WebResponse<IReadOnlyList<UsuarioDto>>.Success(usuarios));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(WebResponse<UsuarioDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(int id, CancellationToken cancellationToken)
    {
        var usuario = await _service.ObterPorIdAsync(id, cancellationToken);
        return Ok(WebResponse<UsuarioDto>.Success(usuario));
    }

    [HttpPost]
    [ProducesResponseType(typeof(WebResponse<UsuarioDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Criar([FromBody] CriarUsuarioDto dto, CancellationToken cancellationToken)
    {
        var usuario = await _service.CriarAsync(dto, cancellationToken);
        var resposta = WebResponse<UsuarioDto>.Success(usuario, StatusCodes.Status201Created);
        return CreatedAtAction(nameof(ObterPorId), new { id = usuario.Id }, resposta);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(WebResponse<UsuarioDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(int id, [FromBody] AtualizarUsuarioDto dto, CancellationToken cancellationToken)
    {
        var usuario = await _service.AtualizarAsync(id, dto, cancellationToken);
        return Ok(WebResponse<UsuarioDto>.Success(usuario));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(WebResponse<UsuarioDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Inativar(int id, CancellationToken cancellationToken)
    {
        var usuario = await _service.InativarAsync(id, cancellationToken);
        return Ok(WebResponse<UsuarioDto>.Success(usuario));
    }
}
