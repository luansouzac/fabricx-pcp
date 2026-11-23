using FabricX.Domain.Entities;

namespace FabricX.Application.Interfaces;

public interface IRoteiroEtapaRepository
{
    Task<RoteiroEtapa?> ObterPorIdAsync(int id, CancellationToken cancellationToken);
    Task<IReadOnlyList<RoteiroEtapa>> ListarPorProdutoAsync(int produtoId, CancellationToken cancellationToken);
    Task AdicionarAsync(RoteiroEtapa etapa, CancellationToken cancellationToken);
    Task AtualizarAsync(RoteiroEtapa etapa, CancellationToken cancellationToken);
    Task RemoverAsync(RoteiroEtapa etapa, CancellationToken cancellationToken);
}
