using FabricX.Application.Interfaces;
using FabricX.Domain.Entities;
using FabricX.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FabricX.Infrastructure.Repositories;

public class RecursoRepository : IRecursoRepository
{
    private readonly AppDbContext _contexto;

    public RecursoRepository(AppDbContext contexto)
    {
        _contexto = contexto;
    }

    public Task<Recurso?> ObterPorIdAsync(int id, CancellationToken cancellationToken) =>
        _contexto.Recursos.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public Task<Recurso?> ObterPorNomeAsync(string nome, CancellationToken cancellationToken) =>
        _contexto.Recursos.AsNoTracking().FirstOrDefaultAsync(r => r.Nome == nome, cancellationToken);

    public async Task<IReadOnlyList<Recurso>> ListarAsync(CancellationToken cancellationToken) =>
        await _contexto.Recursos.AsNoTracking().OrderBy(r => r.Nome).ToListAsync(cancellationToken);

    public async Task AdicionarAsync(Recurso recurso, CancellationToken cancellationToken)
    {
        _contexto.Recursos.Add(recurso);
        await _contexto.SaveChangesAsync(cancellationToken);
    }

    public Task AtualizarAsync(Recurso recurso, CancellationToken cancellationToken) =>
        _contexto.SaveChangesAsync(cancellationToken);
}
