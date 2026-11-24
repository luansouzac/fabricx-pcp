using FabricX.Application.Interfaces;
using FabricX.Domain.Entities;

namespace FabricX.Tests.Fakes;

public class UsuarioRepositoryFake : IUsuarioRepository
{
    private readonly List<Usuario> _usuarios = [];
    private int _proximoId = 1;

    public Task<Usuario?> ObterPorIdAsync(int id, CancellationToken cancellationToken) =>
        Task.FromResult(_usuarios.FirstOrDefault(u => u.Id == id));

    public Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken cancellationToken) =>
        Task.FromResult(_usuarios.FirstOrDefault(u => u.Email == email));

    public Task<IReadOnlyList<Usuario>> ListarAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Usuario>>(_usuarios.ToList());

    public Task AdicionarAsync(Usuario usuario, CancellationToken cancellationToken)
    {
        typeof(Usuario).GetProperty(nameof(Usuario.Id))!.SetValue(usuario, _proximoId++);
        _usuarios.Add(usuario);
        return Task.CompletedTask;
    }

    public Task AtualizarAsync(Usuario usuario, CancellationToken cancellationToken) => Task.CompletedTask;
}
