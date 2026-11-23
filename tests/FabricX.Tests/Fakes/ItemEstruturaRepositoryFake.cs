using FabricX.Application.Interfaces;
using FabricX.Domain.Entities;

namespace FabricX.Tests.Fakes;

public class ItemEstruturaRepositoryFake : IItemEstruturaRepository
{
    private readonly List<ItemEstrutura> _itens = [];

    public IReadOnlyList<ItemEstrutura> Itens => _itens;

    public Task<ItemEstrutura?> ObterAsync(int produtoPaiId, int componenteId, CancellationToken cancellationToken) =>
        Task.FromResult(_itens.FirstOrDefault(i => i.ProdutoPaiId == produtoPaiId && i.ComponenteId == componenteId));

    public Task<IReadOnlyList<ItemEstrutura>> ListarPorPaiAsync(int produtoPaiId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<ItemEstrutura>>(_itens.Where(i => i.ProdutoPaiId == produtoPaiId).ToList());

    public Task<IReadOnlyList<ItemEstrutura>> ListarTodosAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<ItemEstrutura>>(_itens.ToList());

    public Task AdicionarAsync(ItemEstrutura item, CancellationToken cancellationToken)
    {
        _itens.Add(item);
        return Task.CompletedTask;
    }

    public Task AtualizarAsync(ItemEstrutura item, CancellationToken cancellationToken) => Task.CompletedTask;

    public Task RemoverAsync(ItemEstrutura item, CancellationToken cancellationToken)
    {
        _itens.Remove(item);
        return Task.CompletedTask;
    }
}
