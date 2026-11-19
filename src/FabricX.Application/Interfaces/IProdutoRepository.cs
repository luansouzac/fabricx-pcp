using FabricX.Domain.Entities;
using FabricX.Domain.Enums;

namespace FabricX.Application.Interfaces;

public interface IProdutoRepository
{
    Task<Produto?> ObterPorIdAsync(int id, CancellationToken cancellationToken);
    Task<Produto?> ObterPorCodigoAsync(string codigo, CancellationToken cancellationToken);
    Task<IReadOnlyList<Produto>> ListarAsync(TipoProduto? tipo, CancellationToken cancellationToken);
    Task AdicionarAsync(Produto produto, CancellationToken cancellationToken);
    Task AtualizarAsync(Produto produto, CancellationToken cancellationToken);
}
