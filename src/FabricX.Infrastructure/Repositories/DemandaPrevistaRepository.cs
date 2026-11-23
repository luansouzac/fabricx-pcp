using FabricX.Application.Interfaces;
using FabricX.Domain.Entities;
using FabricX.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FabricX.Infrastructure.Repositories;

public class DemandaPrevistaRepository : IDemandaPrevistaRepository
{
    private readonly AppDbContext _contexto;

    public DemandaPrevistaRepository(AppDbContext contexto)
    {
        _contexto = contexto;
    }

    public Task<DemandaPrevista?> ObterPorIdAsync(int id, CancellationToken cancellationToken) =>
        _contexto.DemandasPrevistas.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

    public Task<DemandaPrevista?> ObterPorProdutoEPeriodoAsync(int produtoId, string periodo, CancellationToken cancellationToken) =>
        _contexto.DemandasPrevistas.AsNoTracking().FirstOrDefaultAsync(d => d.ProdutoId == produtoId && d.Periodo == periodo, cancellationToken);

    public async Task<IReadOnlyList<DemandaPrevista>> ListarAsync(string? periodo, CancellationToken cancellationToken) =>
        await _contexto.DemandasPrevistas.AsNoTracking()
            .Where(d => periodo == null || d.Periodo == periodo)
            .OrderBy(d => d.Periodo)
            .ThenBy(d => d.ProdutoId)
            .ToListAsync(cancellationToken);

    public async Task AdicionarAsync(DemandaPrevista demanda, CancellationToken cancellationToken)
    {
        _contexto.DemandasPrevistas.Add(demanda);
        await _contexto.SaveChangesAsync(cancellationToken);
    }

    public Task AtualizarAsync(DemandaPrevista demanda, CancellationToken cancellationToken) =>
        _contexto.SaveChangesAsync(cancellationToken);

    public async Task RemoverAsync(DemandaPrevista demanda, CancellationToken cancellationToken)
    {
        _contexto.DemandasPrevistas.Remove(demanda);
        await _contexto.SaveChangesAsync(cancellationToken);
    }
}
