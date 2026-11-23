using FabricX.Domain.Entities;

namespace FabricX.Application.Interfaces;

public interface IApontamentoRepository
{
    Task<IReadOnlyList<Apontamento>> ListarPorProgramacaoAsync(int programacaoId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Apontamento>> ListarPorOrdemAsync(int ordemId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Apontamento>> ListarAsync(DateTime? de, DateTime? ate, CancellationToken cancellationToken);
    Task AdicionarAsync(Apontamento apontamento, CancellationToken cancellationToken);
}
