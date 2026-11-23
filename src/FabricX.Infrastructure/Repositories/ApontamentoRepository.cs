using FabricX.Application.Interfaces;
using FabricX.Domain.Entities;
using FabricX.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FabricX.Infrastructure.Repositories;

public class ApontamentoRepository : IApontamentoRepository
{
    private readonly AppDbContext _contexto;

    public ApontamentoRepository(AppDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<IReadOnlyList<Apontamento>> ListarPorProgramacaoAsync(int programacaoId, CancellationToken cancellationToken) =>
        await _contexto.Apontamentos.AsNoTracking()
            .Where(a => a.ProgramacaoId == programacaoId)
            .OrderBy(a => a.DataHora)
            .ThenBy(a => a.Id)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Apontamento>> ListarPorOrdemAsync(int ordemId, CancellationToken cancellationToken) =>
        await _contexto.Apontamentos.AsNoTracking()
            .Where(a => _contexto.Programacoes.Any(p => p.Id == a.ProgramacaoId && p.OrdemId == ordemId))
            .OrderBy(a => a.DataHora)
            .ThenBy(a => a.Id)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Apontamento>> ListarAsync(DateTime? de, DateTime? ate, CancellationToken cancellationToken) =>
        await _contexto.Apontamentos.AsNoTracking()
            .Where(a => de == null || a.DataHora >= de)
            .Where(a => ate == null || a.DataHora < ate)
            .OrderBy(a => a.DataHora)
            .ToListAsync(cancellationToken);

    public async Task AdicionarAsync(Apontamento apontamento, CancellationToken cancellationToken)
    {
        _contexto.Apontamentos.Add(apontamento);
        await _contexto.SaveChangesAsync(cancellationToken);
    }
}
