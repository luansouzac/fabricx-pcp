using FabricX.Api.Common;
using FabricX.Application.DTOs.Auth;
using FabricX.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FabricX.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _service;

    public AuthController(IAuthService service)
    {
        _service = service;
    }

    [HttpPost("registro")]
    [ProducesResponseType(typeof(WebResponse<UsuarioDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Registrar([FromBody] RegistrarUsuarioDto dto, CancellationToken cancellationToken)
    {
        var usuario = await _service.RegistrarAsync(dto, cancellationToken);
        var resposta = WebResponse<UsuarioDto>.Success(usuario, StatusCodes.Status201Created, "Usuário cadastrado.");
        return StatusCode(StatusCodes.Status201Created, resposta);
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(WebResponse<TokenDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken cancellationToken)
    {
        var token = await _service.LoginAsync(dto, cancellationToken);
        return Ok(WebResponse<TokenDto>.Success(token, message: "Autenticado."));
    }
}
