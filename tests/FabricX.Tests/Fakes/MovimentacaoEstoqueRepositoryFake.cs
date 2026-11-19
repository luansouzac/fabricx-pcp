using FabricX.Application.Interfaces;
using FabricX.Domain.Entities;

namespace FabricX.Tests.Fakes;

public class MovimentacaoEstoqueRepositoryFake : IMovimentacaoEstoqueRepository
{
    public List<MovimentacaoEstoque> Registradas { get; } = [];

    public Task<IReadOnlyList<MovimentacaoEstoque>> ListarPorProdutoAsync(int produtoId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<MovimentacaoEstoque>>(
            Registradas.Where(m => m.ProdutoId == produtoId).Reverse().ToList());

    public Task AdicionarAsync(MovimentacaoEstoque movimentacao, CancellationToken cancellationToken)
    {
        Registradas.Add(movimentacao);
        return Task.CompletedTask;
    }
}
