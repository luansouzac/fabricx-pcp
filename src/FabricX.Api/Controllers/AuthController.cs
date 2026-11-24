using FabricX.Api.Common;
using FabricX.Application.DTOs.Auth;
using FabricX.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FabricX.Api.Controllers;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IAuthService _service;

    public AuthController(IAuthService service)
    {
        _service = service;
    }

    [HttpPost("login")]
    [EnableRateLimiting(Limitador.Login)]
    [ProducesResponseType(typeof(WebResponse<TokenDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken cancellationToken)
    {
        var token = await _service.LoginAsync(dto, cancellationToken);
        return Ok(WebResponse<TokenDto>.Success(token, message: "Autenticado."));
    }
}
