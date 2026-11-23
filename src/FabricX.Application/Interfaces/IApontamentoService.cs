using FabricX.Application.DTOs.Apontamentos;

namespace FabricX.Application.Interfaces;

public interface IApontamentoService
{
    Task<ApontamentoDto> RegistrarAsync(int programacaoId, RegistrarApontamentoDto dto, CancellationToken cancellationToken);
    Task<IReadOnlyList<ApontamentoDto>> ListarPorProgramacaoAsync(int programacaoId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ApontamentoDto>> ListarPorOrdemAsync(int ordemId, CancellationToken cancellationToken);
}
