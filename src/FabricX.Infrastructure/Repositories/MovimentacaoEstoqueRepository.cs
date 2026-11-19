using FabricX.Application.Interfaces;
using FabricX.Domain.Entities;
using FabricX.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FabricX.Infrastructure.Repositories;

public class MovimentacaoEstoqueRepository : IMovimentacaoEstoqueRepository
{
    private readonly AppDbContext _contexto;

    public MovimentacaoEstoqueRepository(AppDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<IReadOnlyList<MovimentacaoEstoque>> ListarPorProdutoAsync(int produtoId, CancellationToken cancellationToken) =>
        await _contexto.MovimentacoesEstoque.AsNoTracking()
            .Where(m => m.ProdutoId == produtoId)
            .OrderByDescending(m => m.DataHora)
            .ThenByDescending(m => m.Id)
            .ToListAsync(cancellationToken);

    // O SaveChanges grava junto o saldo do produto já alterado no mesmo AppDbContext (scoped), em uma transação só.
    public async Task AdicionarAsync(MovimentacaoEstoque movimentacao, CancellationToken cancellationToken)
    {
        _contexto.MovimentacoesEstoque.Add(movimentacao);
        await _contexto.SaveChangesAsync(cancellationToken);
    }
}
