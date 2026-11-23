using FabricX.Domain.Entities;

namespace FabricX.Application.Interfaces;

public interface IMovimentacaoEstoqueRepository
{
    Task<IReadOnlyList<MovimentacaoEstoque>> ListarPorProdutoAsync(int produtoId, CancellationToken cancellationToken);
    Task AdicionarAsync(MovimentacaoEstoque movimentacao, CancellationToken cancellationToken);
}
