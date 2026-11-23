using FabricX.Application.DTOs.Capacidade;

namespace FabricX.Application.Interfaces;

public interface ICapacidadeService
{
    Task<CapacidadeDto> CalcularAsync(string? periodo, CancellationToken cancellationToken);
}
