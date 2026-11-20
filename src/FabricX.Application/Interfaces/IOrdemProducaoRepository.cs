using FabricX.Domain.Entities;
using FabricX.Domain.Enums;

namespace FabricX.Application.Interfaces;

public interface IOrdemProducaoRepository
{
    Task<OrdemProducao?> ObterPorIdAsync(int id, CancellationToken cancellationToken);
    Task<IReadOnlyList<OrdemProducao>> ListarAsync(StatusOrdem? status, CancellationToken cancellationToken);
    Task<int> ObterMaiorSequencialDoAnoAsync(int ano, CancellationToken cancellationToken);
    Task AdicionarAsync(OrdemProducao ordem, CancellationToken cancellationToken);
    Task AtualizarAsync(OrdemProducao ordem, CancellationToken cancellationToken);
}
