using FabricX.Application.DTOs.Demanda;

namespace FabricX.Application.Interfaces;

public interface IDemandaPrevistaService
{
    Task<IReadOnlyList<DemandaPrevistaDto>> ListarAsync(string? periodo, CancellationToken cancellationToken);
    Task<DemandaPrevistaDto> CriarAsync(CriarDemandaPrevistaDto dto, CancellationToken cancellationToken);
    Task<DemandaPrevistaDto> AtualizarAsync(int id, AtualizarDemandaPrevistaDto dto, CancellationToken cancellationToken);
    Task<DemandaPrevistaDto> RemoverAsync(int id, CancellationToken cancellationToken);
}
