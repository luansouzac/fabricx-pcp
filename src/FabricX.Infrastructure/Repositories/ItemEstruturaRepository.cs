using FabricX.Application.Interfaces;
using FabricX.Domain.Entities;
using FabricX.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FabricX.Infrastructure.Repositories;

public class ItemEstruturaRepository : IItemEstruturaRepository
{
    private readonly AppDbContext _contexto;

    public ItemEstruturaRepository(AppDbContext contexto)
    {
        _contexto = contexto;
    }

    public Task<ItemEstrutura?> ObterAsync(int produtoPaiId, int componenteId, CancellationToken cancellationToken) =>
        _contexto.ItensEstrutura.FirstOrDefaultAsync(
            i => i.ProdutoPaiId == produtoPaiId && i.ComponenteId == componenteId, cancellationToken);

    public async Task<IReadOnlyList<ItemEstrutura>> ListarPorPaiAsync(int produtoPaiId, CancellationToken cancellationToken) =>
        await _contexto.ItensEstrutura.AsNoTracking()
            .Where(i => i.ProdutoPaiId == produtoPaiId)
            .OrderBy(i => i.Id)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ItemEstrutura>> ListarTodosAsync(CancellationToken cancellationToken) =>
        await _contexto.ItensEstrutura.AsNoTracking().ToListAsync(cancellationToken);

    public async Task AdicionarAsync(ItemEstrutura item, CancellationToken cancellationToken)
    {
        _contexto.ItensEstrutura.Add(item);
        await _contexto.SaveChangesAsync(cancellationToken);
    }

    public Task AtualizarAsync(ItemEstrutura item, CancellationToken cancellationToken) =>
        _contexto.SaveChangesAsync(cancellationToken);

    public async Task RemoverAsync(ItemEstrutura item, CancellationToken cancellationToken)
    {
        _contexto.ItensEstrutura.Remove(item);
        await _contexto.SaveChangesAsync(cancellationToken);
    }
}
