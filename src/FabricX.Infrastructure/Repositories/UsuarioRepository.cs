using FabricX.Application.Interfaces;
using FabricX.Domain.Entities;
using FabricX.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FabricX.Infrastructure.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly AppDbContext _contexto;

    public UsuarioRepository(AppDbContext contexto)
    {
        _contexto = contexto;
    }

    public Task<Usuario?> ObterPorIdAsync(int id, CancellationToken cancellationToken) =>
        _contexto.Usuarios.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken cancellationToken) =>
        _contexto.Usuarios.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public async Task<IReadOnlyList<Usuario>> ListarAsync(CancellationToken cancellationToken) =>
        await _contexto.Usuarios.AsNoTracking().OrderBy(u => u.Nome).ToListAsync(cancellationToken);

    public async Task AdicionarAsync(Usuario usuario, CancellationToken cancellationToken)
    {
        _contexto.Usuarios.Add(usuario);
        await _contexto.SaveChangesAsync(cancellationToken);
    }

    public Task AtualizarAsync(Usuario usuario, CancellationToken cancellationToken) =>
        _contexto.SaveChangesAsync(cancellationToken);
}
