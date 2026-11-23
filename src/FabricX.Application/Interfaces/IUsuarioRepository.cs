using FabricX.Domain.Entities;

namespace FabricX.Application.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario?> ObterPorIdAsync(int id, CancellationToken cancellationToken);
    Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken cancellationToken);
    Task<IReadOnlyList<Usuario>> ListarAsync(CancellationToken cancellationToken);
    Task AdicionarAsync(Usuario usuario, CancellationToken cancellationToken);
    Task AtualizarAsync(Usuario usuario, CancellationToken cancellationToken);
}
