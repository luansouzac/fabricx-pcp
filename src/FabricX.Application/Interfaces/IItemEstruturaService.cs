using FabricX.Application.DTOs.Estrutura;

namespace FabricX.Application.Interfaces;

public interface IItemEstruturaService
{
    Task<IReadOnlyList<ItemEstruturaDto>> ListarAsync(int produtoPaiId, CancellationToken cancellationToken);
    Task<ItemEstruturaDto> AdicionarAsync(int produtoPaiId, AdicionarItemEstruturaDto dto, CancellationToken cancellationToken);
    Task<ItemEstruturaDto> AlterarQuantidadeAsync(int produtoPaiId, int componenteId, AlterarQuantidadeItemEstruturaDto dto, CancellationToken cancellationToken);
    Task<ItemEstruturaDto> RemoverAsync(int produtoPaiId, int componenteId, CancellationToken cancellationToken);
}
