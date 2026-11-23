using FabricX.Domain.Entities;

namespace FabricX.Application.Interfaces;

public interface IDemandaPrevistaRepository
{
    Task<DemandaPrevista?> ObterPorIdAsync(int id, CancellationToken cancellationToken);
    Task<DemandaPrevista?> ObterPorProdutoEPeriodoAsync(int produtoId, string periodo, CancellationToken cancellationToken);
    Task<IReadOnlyList<DemandaPrevista>> ListarAsync(string? periodo, CancellationToken cancellationToken);
    Task AdicionarAsync(DemandaPrevista demanda, CancellationToken cancellationToken);
    Task AtualizarAsync(DemandaPrevista demanda, CancellationToken cancellationToken);
    Task RemoverAsync(DemandaPrevista demanda, CancellationToken cancellationToken);
}
