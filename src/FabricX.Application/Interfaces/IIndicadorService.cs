using FabricX.Application.DTOs.Indicadores;

namespace FabricX.Application.Interfaces;

public interface IIndicadorService
{
    Task<IndicadoresDto> CalcularAsync(DateOnly? de, DateOnly? ate, int? produtoId, int? recursoId, CancellationToken cancellationToken);
    Task<ResumoDto> ObterResumoAsync(CancellationToken cancellationToken);
}
