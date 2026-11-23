using FabricX.Application.DTOs.Recursos;

namespace FabricX.Application.Interfaces;

public interface IRecursoService
{
    Task<IReadOnlyList<RecursoDto>> ListarAsync(CancellationToken cancellationToken);
    Task<RecursoDto> ObterPorIdAsync(int id, CancellationToken cancellationToken);
    Task<RecursoDto> CriarAsync(CriarRecursoDto dto, CancellationToken cancellationToken);
    Task<RecursoDto> AtualizarAsync(int id, AtualizarRecursoDto dto, CancellationToken cancellationToken);
    Task<RecursoDto> InativarAsync(int id, CancellationToken cancellationToken);
}
