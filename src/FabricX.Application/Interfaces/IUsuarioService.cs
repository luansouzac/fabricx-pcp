using FabricX.Application.DTOs.Auth;

namespace FabricX.Application.Interfaces;

public interface IUsuarioService
{
    Task<IReadOnlyList<UsuarioDto>> ListarAsync(CancellationToken cancellationToken);
    Task<UsuarioDto> ObterPorIdAsync(int id, CancellationToken cancellationToken);
    Task<UsuarioDto> CriarAsync(CriarUsuarioDto dto, CancellationToken cancellationToken);
    Task<UsuarioDto> AtualizarAsync(int id, AtualizarUsuarioDto dto, CancellationToken cancellationToken);
    Task<UsuarioDto> InativarAsync(int id, CancellationToken cancellationToken);
}
