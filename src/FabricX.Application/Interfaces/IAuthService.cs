using FabricX.Application.DTOs.Auth;

namespace FabricX.Application.Interfaces;

public interface IAuthService
{
    Task<TokenDto> LoginAsync(LoginDto dto, CancellationToken cancellationToken);
}
