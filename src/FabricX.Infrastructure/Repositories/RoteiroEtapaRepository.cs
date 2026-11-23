using FabricX.Application.Interfaces;
using FabricX.Domain.Entities;
using FabricX.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FabricX.Infrastructure.Repositories;

public class RoteiroEtapaRepository : IRoteiroEtapaRepository
{
    private readonly AppDbContext _contexto;

    public RoteiroEtapaRepository(AppDbContext contexto)
    {
        _contexto = contexto;
    }

    public Task<RoteiroEtapa?> ObterPorIdAsync(int id, CancellationToken cancellationToken) =>
        _contexto.EtapasRoteiro.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<IReadOnlyList<RoteiroEtapa>> ListarPorProdutoAsync(int produtoId, CancellationToken cancellationToken) =>
        await _contexto.EtapasRoteiro.AsNoTracking()
            .Where(e => e.ProdutoId == produtoId)
            .OrderBy(e => e.Sequencia)
            .ToListAsync(cancellationToken);

    public async Task AdicionarAsync(RoteiroEtapa etapa, CancellationToken cancellationToken)
    {
        _contexto.EtapasRoteiro.Add(etapa);
        await _contexto.SaveChangesAsync(cancellationToken);
    }

    public Task AtualizarAsync(RoteiroEtapa etapa, CancellationToken cancellationToken) =>
        _contexto.SaveChangesAsync(cancellationToken);

    public async Task RemoverAsync(RoteiroEtapa etapa, CancellationToken cancellationToken)
    {
        _contexto.EtapasRoteiro.Remove(etapa);
        await _contexto.SaveChangesAsync(cancellationToken);
    }
}
