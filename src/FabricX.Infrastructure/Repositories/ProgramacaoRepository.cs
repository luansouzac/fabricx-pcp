using FabricX.Application.Interfaces;
using FabricX.Domain.Entities;
using FabricX.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FabricX.Infrastructure.Repositories;

public class ProgramacaoRepository : IProgramacaoRepository
{
    private readonly AppDbContext _contexto;

    public ProgramacaoRepository(AppDbContext contexto)
    {
        _contexto = contexto;
    }

    public Task<Programacao?> ObterPorIdAsync(int id, CancellationToken cancellationToken) =>
        _contexto.Programacoes.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Programacao>> ListarPorOrdemAsync(int ordemId, CancellationToken cancellationToken) =>
        await _contexto.Programacoes.AsNoTracking()
            .Where(p => p.OrdemId == ordemId)
            .OrderBy(p => p.InicioPrevisto)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Programacao>> ListarPorRecursoAsync(int recursoId, CancellationToken cancellationToken) =>
        await _contexto.Programacoes.AsNoTracking()
            .Where(p => p.RecursoId == recursoId)
            .OrderBy(p => p.InicioPrevisto)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Programacao>> ListarAsync(int? recursoId, DateTime? de, DateTime? ate, CancellationToken cancellationToken) =>
        await _contexto.Programacoes.AsNoTracking()
            .Where(p => recursoId == null || p.RecursoId == recursoId)
            .Where(p => de == null || p.FimPrevisto > de)
            .Where(p => ate == null || p.InicioPrevisto < ate)
            .OrderBy(p => p.InicioPrevisto)
            .ThenBy(p => p.Id)
            .ToListAsync(cancellationToken);

    public async Task AdicionarAsync(Programacao programacao, CancellationToken cancellationToken)
    {
        _contexto.Programacoes.Add(programacao);
        await _contexto.SaveChangesAsync(cancellationToken);
    }

    public Task AtualizarAsync(Programacao programacao, CancellationToken cancellationToken) =>
        _contexto.SaveChangesAsync(cancellationToken);

    public async Task RemoverAsync(Programacao programacao, CancellationToken cancellationToken)
    {
        _contexto.Programacoes.Remove(programacao);
        await _contexto.SaveChangesAsync(cancellationToken);
    }
}
