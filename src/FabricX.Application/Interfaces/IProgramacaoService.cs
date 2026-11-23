using FabricX.Application.DTOs.Programacao;

namespace FabricX.Application.Interfaces;

public interface IProgramacaoService
{
    Task<ProgramacaoDto> AgendarAsync(int ordemId, AgendarEtapaDto dto, CancellationToken cancellationToken);
    Task<IReadOnlyList<ProgramacaoDto>> ListarPorOrdemAsync(int ordemId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ProgramacaoDto>> ListarCronogramaAsync(int? recursoId, DateOnly? de, DateOnly? ate, CancellationToken cancellationToken);
    Task<ProgramacaoDto> ReprogramarAsync(int id, ReprogramarEtapaDto dto, CancellationToken cancellationToken);
    Task<ProgramacaoDto> RemoverAsync(int id, CancellationToken cancellationToken);
}
