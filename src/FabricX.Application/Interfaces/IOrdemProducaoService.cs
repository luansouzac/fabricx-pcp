using FabricX.Application.DTOs.Ordens;
using FabricX.Domain.Enums;

namespace FabricX.Application.Interfaces;

public interface IOrdemProducaoService
{
    Task<IReadOnlyList<OrdemProducaoDto>> ListarAsync(StatusOrdem? status, CancellationToken cancellationToken);
    Task<OrdemProducaoDto> ObterPorIdAsync(int id, CancellationToken cancellationToken);
    Task<OrdemProducaoDto> CriarAsync(CriarOrdemProducaoDto dto, CancellationToken cancellationToken);
    Task<OrdemProducaoDto> AlterarAsync(int id, AlterarOrdemProducaoDto dto, CancellationToken cancellationToken);
    Task<OrdemProducaoDto> AvancarAsync(int id, CancellationToken cancellationToken);
    Task<OrdemProducaoDto> CancelarAsync(int id, CancellationToken cancellationToken);
}
