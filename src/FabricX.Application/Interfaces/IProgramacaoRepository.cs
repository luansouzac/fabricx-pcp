using FabricX.Domain.Entities;

namespace FabricX.Application.Interfaces;

public interface IProgramacaoRepository
{
    Task<Programacao?> ObterPorIdAsync(int id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Programacao>> ListarPorOrdemAsync(int ordemId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Programacao>> ListarPorRecursoAsync(int recursoId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Programacao>> ListarAsync(int? recursoId, DateTime? de, DateTime? ate, CancellationToken cancellationToken);
    Task AdicionarAsync(Programacao programacao, CancellationToken cancellationToken);
    Task AtualizarAsync(Programacao programacao, CancellationToken cancellationToken);
    Task RemoverAsync(Programacao programacao, CancellationToken cancellationToken);
}
