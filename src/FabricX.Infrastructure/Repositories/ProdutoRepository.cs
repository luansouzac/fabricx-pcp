using FabricX.Application.Interfaces;
using FabricX.Domain.Entities;
using FabricX.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FabricX.Infrastructure.Repositories;

public class ProdutoRepository : IProdutoRepository
{
    private readonly AppDbContext _contexto;

    public ProdutoRepository(AppDbContext contexto)
    {
        _contexto = contexto;
    }

    public Task<Produto?> ObterPorIdAsync(int id, CancellationToken cancellationToken) =>
        _contexto.Produtos.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<Produto?> ObterPorCodigoAsync(string codigo, CancellationToken cancellationToken) =>
        _contexto.Produtos.AsNoTracking().FirstOrDefaultAsync(p => p.Codigo == codigo, cancellationToken);

    public async Task<IReadOnlyList<Produto>> ListarAsync(CancellationToken cancellationToken) =>
        await _contexto.Produtos.AsNoTracking().OrderBy(p => p.Nome).ToListAsync(cancellationToken);

    public async Task AdicionarAsync(Produto produto, CancellationToken cancellationToken)
    {
        _contexto.Produtos.Add(produto);
        await _contexto.SaveChangesAsync(cancellationToken);
    }

    public Task AtualizarAsync(Produto produto, CancellationToken cancellationToken) =>
        _contexto.SaveChangesAsync(cancellationToken);
}
