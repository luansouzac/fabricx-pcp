using FabricX.Domain.Entities;

namespace FabricX.Application.Interfaces;

public interface IItemEstruturaRepository
{
    Task<ItemEstrutura?> ObterAsync(int produtoPaiId, int componenteId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ItemEstrutura>> ListarPorPaiAsync(int produtoPaiId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ItemEstrutura>> ListarTodosAsync(CancellationToken cancellationToken);
    Task AdicionarAsync(ItemEstrutura item, CancellationToken cancellationToken);
    Task AtualizarAsync(ItemEstrutura item, CancellationToken cancellationToken);
    Task RemoverAsync(ItemEstrutura item, CancellationToken cancellationToken);
}
