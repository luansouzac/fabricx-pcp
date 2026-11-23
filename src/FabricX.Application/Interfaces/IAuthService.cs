using FabricX.Application.DTOs.Auth;

namespace FabricX.Application.Interfaces;

public interface IAuthService
{
    Task<UsuarioDto> RegistrarAsync(RegistrarUsuarioDto dto, CancellationToken cancellationToken);
    Task<TokenDto> LoginAsync(LoginDto dto, CancellationToken cancellationToken);
}
