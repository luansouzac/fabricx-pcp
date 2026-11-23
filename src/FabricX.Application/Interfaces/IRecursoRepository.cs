using FabricX.Domain.Entities;

namespace FabricX.Application.Interfaces;

public interface IRecursoRepository
{
    Task<Recurso?> ObterPorIdAsync(int id, CancellationToken cancellationToken);
    Task<Recurso?> ObterPorNomeAsync(string nome, CancellationToken cancellationToken);
    Task<IReadOnlyList<Recurso>> ListarAsync(CancellationToken cancellationToken);
    Task AdicionarAsync(Recurso recurso, CancellationToken cancellationToken);
    Task AtualizarAsync(Recurso recurso, CancellationToken cancellationToken);
}
